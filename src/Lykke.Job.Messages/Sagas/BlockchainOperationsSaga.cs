using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainCashoutProcessor.Contract.Events;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Resources;
using Lykke.Service.ClientAccount.Client;
using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.Messages.Core.Domain.Deduplication;
using Lykke.Job.Messages.Core.Util;
using Lykke.Service.Assets.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.PushNotifications.Contract.Commands;
using Lykke.Service.TemplateFormatter.Client;

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
            await SendCashinEmailAsync(evt.OperationId, evt.ClientId, evt.Amount, evt.AssetId, commandSender);
        }

        //From Sirius DepositsDetector
        [UsedImplicitly]
        public async Task Handle(SiriusDepositsDetector.Contract.Events.CashinCompletedEvent evt, ICommandSender commandSender)
        {
            await SendCashinEmailAsync(evt.OperationId, Guid.Parse(evt.ClientId), evt.Amount, evt.AssetId, commandSender);
        }

        #region CashoutProcessor

        [UsedImplicitly]
        public async Task Handle(CrossClientCashoutCompletedEvent evt, ICommandSender commandSender)
        {
            //Cross client means we change ME Balance and do not broadcast any transactions
            //Send confirmation to sender that cashout is completed
            await SendCashoutEmailAsync(evt.OperationId, evt.ClientId, evt.Amount, evt.AssetId, commandSender);
            //Send confirmation to recepient that cashin is completed
            await SendCashinEmailAsync(evt.CashinOperationId, evt.RecipientClientId, evt.Amount, evt.AssetId, commandSender);
        }

        [UsedImplicitly]
        public async Task Handle(CashoutCompletedEvent evt, ICommandSender commandSender)
        {
            await SendCashoutEmailAsync(evt.OperationId, evt.ClientId, evt.Amount, evt.AssetId, commandSender);
        }

        [UsedImplicitly]
        public async Task Handle(SiriusCashoutProcessor.Contract.Events.CashoutCompletedEvent evt, ICommandSender commandSender)
        {
            await SendCashoutEmailAsync(evt.OperationId, evt.ClientId, evt.Amount, evt.AssetId, commandSender);
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
                await SendCashoutEmailAsync(cashout.OperationId, cashout.ClientId, cashout.Amount, evt.AssetId, commandSender);
            }
        }

        #endregion

        private async Task SendCashinEmailAsync(Guid operationId, Guid clientId, decimal amount, string assetId, ICommandSender commandSender)
        {
            if (await _deduplicationRepository.IsExistsAsync(operationId))
                return;

            var clientModel = await _clientAccountClient.GetByIdAsync(clientId.ToString());

            if (clientModel == null)
            {
                var exception = new InvalidOperationException($"Client not found(clientId = { clientId })");
                _log.Error(nameof(SendCashinEmailAsync), exception);

                throw exception;
            }

            var asset = await _cachedAssetsService.TryGetAssetAsync(assetId);

            if (asset == null)
            {
                var exception = new InvalidOperationException($"Asset not found (assetId = {assetId})");
                _log.Error(nameof(SendCashinEmailAsync), exception);

                throw exception;

            }

            string amountFormatted = NumberFormatter.FormatNumber(amount, asset.Accuracy);

            var parameters = new
            {
                AssetName = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId,
                Amount = amountFormatted,
                Year = DateTime.UtcNow.Year.ToString()
            };

            commandSender.SendCommand(new SendEmailCommand
            {
                ApplicationId = clientModel.PartnerId,
                Template = "NoRefundDepositDoneTemplate",
                EmailAddresses = new[] {clientModel.Email},
                Payload = parameters
            }, EmailMessagesBoundedContext.Name);

            var pushSettings = await _clientAccountClient.GetPushNotificationAsync(clientId.ToString());

            if (!pushSettings.Enabled || string.IsNullOrEmpty(clientModel.NotificationsId))
                return;

            var template = await _templateFormatter.FormatAsync("PushDepositCompletedTemplate", clientModel.PartnerId, "EN",
                new
                {
                    Amount = amountFormatted,
                    AssetDisplayId = asset.DisplayId
                });

            if (template != null)
            {
                commandSender.SendCommand(new AssetsCreditedCommand
                {
                    Amount = (double) amount,
                    AssetId = assetId,
                    Message = template.Subject,
                    NotificationIds = new[] {clientModel.NotificationsId}
                }, PushNotificationsBoundedContext.Name);
            }

            await _deduplicationRepository.InsertOrReplaceAsync(operationId);
        }

        public async Task SendCashoutEmailAsync(Guid operationId, Guid clientId, decimal amount, string assetId, ICommandSender commandSender)
        {
            if (await _deduplicationRepository.IsExistsAsync(operationId))
                return;

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

            var parameters = new
            {
                AssetId = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId,
                Amount = NumberFormatter.FormatNumber(amount, asset.Accuracy),
                ExplorerUrl = "",
                //{ "ExplorerUrl", string.Format(_blockchainSettings.ExplorerUrl, messageData.SrcBlockchainHash },
                Year = DateTime.UtcNow.Year.ToString()
            };

            commandSender.SendCommand(new SendEmailCommand
                {
                    ApplicationId = clientModel.PartnerId,
                    Template = "NoRefundOCashOutTemplate",
                    EmailAddresses = new[] { clientModel.Email },
                    Payload = parameters
                },
                EmailMessagesBoundedContext.Name);

            await _deduplicationRepository.InsertOrReplaceAsync(operationId);
        }
    }
}
