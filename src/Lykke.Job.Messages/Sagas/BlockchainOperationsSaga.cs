using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainCashoutProcessor.Contract.Events;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Resources;
using Lykke.Service.ClientAccount.Client;
using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.Messages.Core.Domain.Deduplication;
using Lykke.Job.Messages.Core.Util;
using Lykke.Service.Assets.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.PushNotifications.Contract.Commands;
using Lykke.Service.TemplateFormatter.Client;
using Microsoft.Extensions.Logging;

namespace Lykke.Job.Messages.Sagas
{
    //Listens On ME Rabbit
    public class BlockchainOperationsSaga
    {
        private readonly IAssetsServiceWithCache _cachedAssetsService;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IOperationMessagesDeduplicationRepository _deduplicationRepository;
        private readonly ITemplateFormatter _templateFormatter;
        private readonly ILog _log;

        public BlockchainOperationsSaga(
            IAssetsServiceWithCache cachedAssetsService,
            IClientAccountClient clientAccountClient,
            IOperationMessagesDeduplicationRepository deduplicationRepository,
            ITemplateFormatter templateFormatter,
            ILogFactory logFactory)
        {
            _cachedAssetsService = cachedAssetsService;
            _clientAccountClient = clientAccountClient;
            _deduplicationRepository = deduplicationRepository;
            _templateFormatter = templateFormatter;
            _log = logFactory.CreateLog(this);
        }

        //From CashinDetector
        [UsedImplicitly]
        public async Task Handle(BlockchainCashinDetector.Contract.Events.CashinCompletedEvent evt, ICommandSender commandSender)
        {
            await SendCashinEmailAsync(evt.OperationId, evt.ClientId, null, evt.Amount, evt.AssetId, commandSender);
        }

        //From Sirius DepositsDetector
        [UsedImplicitly]
        public async Task Handle(SiriusDepositsDetector.Contract.Events.CashinCompletedEvent evt, ICommandSender commandSender)
        {
            var walletId = string.IsNullOrWhiteSpace(evt.WalletId) ? (Guid?)null : Guid.Parse(evt.WalletId);
            await SendCashinEmailAsync(evt.OperationId, Guid.Parse(evt.ClientId), walletId, evt.Amount, evt.AssetId, commandSender);
        }
        
        //From InterestPayout
        [UsedImplicitly]
        public async Task Handle(InterestPayout.MessagingContract.PayoutCompletedEvent evt, ICommandSender commandSender)
        {
            var operationId = Guid.Parse(evt.OperationId);

            var isTradingWallet = evt.WalletId == evt.ClientId;
            var walletId = isTradingWallet? (Guid?)null : Guid.Parse(evt.WalletId);
            var clientId = Guid.Parse(evt.ClientId);

            if (!evt.ShouldNotifyUser)
            {
                _log.Info(nameof(InterestPayout.MessagingContract.PayoutCompletedEvent),
                    "PayoutCompletedEvent was received, but notifications are disabled",
                    context: new { evt.WalletId, evt.ClientId, evt.AssetId, evt.OperationId }.ToJson());
                return;
            }
            
            if (evt.Amount > 0)
            {
                await SendCashinEmailAsync(operationId, clientId, walletId, evt.Amount, evt.AssetId, commandSender);
            }
            else
            {
                await SendCashoutEmailAsync(operationId, clientId, walletId, evt.Amount, evt.AssetId, commandSender);
            }
        }

        #region CashoutProcessor

        [UsedImplicitly]
        public async Task Handle(CrossClientCashoutCompletedEvent evt, ICommandSender commandSender)
        {
            //Cross client means we change ME Balance and do not broadcast any transactions
            //Send confirmation to sender that cashout is completed
            await SendCashoutEmailAsync(evt.OperationId, evt.ClientId, null, evt.Amount, evt.AssetId, commandSender);
            //Send confirmation to recepient that cashin is completed
            await SendCashinEmailAsync(evt.CashinOperationId, evt.RecipientClientId, null, evt.Amount, evt.AssetId, commandSender);
        }

        [UsedImplicitly]
        public async Task Handle(CashoutCompletedEvent evt, ICommandSender commandSender)
        {
            await SendCashoutEmailAsync(evt.OperationId, evt.ClientId, null, evt.Amount, evt.AssetId, commandSender);
        }

        [UsedImplicitly]
        public async Task Handle(SiriusCashoutProcessor.Contract.Events.CashoutCompletedEvent evt, ICommandSender commandSender)
        {
            await SendCashoutEmailAsync(evt.OperationId, evt.ClientId, evt.WalletId, evt.Amount, evt.AssetId, commandSender);
        }

        [UsedImplicitly]
        public async Task Handle(CashoutsBatchCompletedEvent evt, ICommandSender commandSender)
        {
            if (evt.Cashouts == null || evt.Cashouts.Length == 0)
            {
                throw new InvalidOperationException($"Batch cashouts are empty. BatchId {evt.BatchId}");
            }

            foreach (var cashout in evt.Cashouts)
            {
                await SendCashoutEmailAsync(cashout.OperationId, cashout.ClientId, null, cashout.Amount, evt.AssetId, commandSender);
            }
        }

        #endregion

        private async Task SendCashinEmailAsync(Guid operationId, Guid clientId, Guid? walletId, decimal amount, string assetId, ICommandSender commandSender)
        {
            if (await _deduplicationRepository.IsExistsAsync(operationId))
                return;

            if (walletId.HasValue && walletId.Value == Guid.Empty)
                walletId = null;

            var clientModel = await _clientAccountClient.GetByIdAsync(clientId.ToString());

            if (clientModel == null)
            {
                throw new InvalidOperationException($"Client not found(clientId = { clientId })");
            }

            var asset = await _cachedAssetsService.TryGetAssetAsync(assetId);

            if (asset == null)
            {
                throw new InvalidOperationException($"Asset not found (assetId = {assetId})");
            }

            string amountFormatted = NumberFormatter.FormatNumber(amount, asset.Accuracy);
            string walletName = walletId.HasValue
                ? (await _clientAccountClient.GetWalletAsync(walletId.Value.ToString())).Name
                : string.Empty;

            var parameters = new
            {
                AssetName = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId,
                Amount = amountFormatted,
                Year = DateTime.UtcNow.Year.ToString(),
                WalletName = walletName
            };

            var emailTemplateName = walletId.HasValue ? "NoRefundDepositApiWalletDoneTemplate" : "NoRefundDepositDoneTemplate";

            commandSender.SendCommand(new SendEmailCommand
            {
                ApplicationId = clientModel.PartnerId,
                Template = emailTemplateName,
                EmailAddresses = new[] {clientModel.Email},
                Payload = parameters
            }, EmailMessagesBoundedContext.Name);

            var pushSettings = await _clientAccountClient.GetPushNotificationAsync(clientId.ToString());

            if (!pushSettings.Enabled || string.IsNullOrEmpty(clientModel.NotificationsId))
                return;

            var pushTemplateName = walletId.HasValue
                ? "PushDepositAPIWalletCompletedTemplate"
                : "PushDepositCompletedTemplate";
            var pushTemplate = await _templateFormatter.FormatAsync(pushTemplateName, clientModel.PartnerId, "EN",
                new
                {
                    Amount = amountFormatted,
                    AssetDisplayId = asset.DisplayId,
                    WalletName = walletName
                });

            if (pushTemplate != null)
            {
                commandSender.SendCommand(new AssetsCreditedCommand
                {
                    Amount = (double) amount,
                    AssetId = assetId,
                    Message = pushTemplate.Subject,
                    NotificationIds = new[] {clientModel.NotificationsId}
                }, PushNotificationsBoundedContext.Name);
            }

            await _deduplicationRepository.InsertOrReplaceAsync(operationId);
        }

        public async Task SendCashoutEmailAsync(Guid operationId, Guid clientId, Guid? walletId, decimal amount, string assetId, ICommandSender commandSender)
        {
            if (await _deduplicationRepository.IsExistsAsync(operationId))
                return;

            if (walletId.HasValue && walletId.Value == Guid.Empty)
                walletId = null;

            var clientModel = await _clientAccountClient.GetByIdAsync(clientId.ToString());
            if (clientModel == null)
            {
                var exception = new InvalidOperationException($"Client not found(clientId = { clientId })");
                _log.Error(nameof(SendCashoutEmailAsync), exception);

                throw exception;
            }

            var asset = await _cachedAssetsService.TryGetAssetAsync(assetId);
            if (asset == null)
            {
                var exception = new InvalidOperationException($"Asset not found (assetId = {assetId})");
                _log.Error(nameof(SendCashoutEmailAsync), exception);

                throw exception;
            }

            if (asset.IsDisabled)
            {
                _log.Info("Asset is disabled, skip cashout notification.", new { assetId = asset.Id});
                return;
            }

            var parameters = new
            {
                AssetId = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId,
                Amount = NumberFormatter.FormatNumber(amount, asset.Accuracy),
                ExplorerUrl = "",
                //{ "ExplorerUrl", string.Format(_blockchainSettings.ExplorerUrl, messageData.SrcBlockchainHash },
                Year = DateTime.UtcNow.Year.ToString(),
                WalletName = walletId.HasValue ? (await _clientAccountClient.GetWalletAsync(walletId.Value.ToString())).Name : string.Empty
            };

            var template = walletId.HasValue ? "NoRefundOCashOutApiWalletTemplate" : "NoRefundOCashOutTemplate";

            commandSender.SendCommand(new SendEmailCommand
                {
                    ApplicationId = clientModel.PartnerId,
                    Template = template,
                    EmailAddresses = new[] { clientModel.Email },
                    Payload = parameters
                },
                EmailMessagesBoundedContext.Name);

            await _deduplicationRepository.InsertOrReplaceAsync(operationId);
        }
    }
}
