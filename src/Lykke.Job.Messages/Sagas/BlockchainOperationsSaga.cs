﻿using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainCashoutProcessor.Contract.Events;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Core.Util;
using Lykke.Job.Messages.Resources;
using Lykke.Job.Messages.Utils;
using Lykke.Service.Assets.Client;
using Lykke.Service.ClientAccount.Client;
using System;
using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Deduplication;
using Lykke.Job.Messages.Core.Util;
using Lykke.Service.Assets.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.PushNotifications.Contract.Commands;
using System;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Sagas
{
    //Listens On ME Rabbit
    public class BlockchainOperationsSaga
    {        
        private readonly IAssetsServiceWithCache _cachedAssetsService;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IPersonalDataService _personalDataService;
        private readonly IOperationMessagesDeduplicationRepository _deduplicationRepository;

        public BlockchainOperationsSaga(            
            IAssetsServiceWithCache cachedAssetsService,
            IClientAccountClient clientAccountClient,
            IPersonalDataService personalDataService,
            IOperationMessagesDeduplicationRepository deduplicationRepository)
        {            
            _cachedAssetsService = cachedAssetsService;
            _clientAccountClient = clientAccountClient;
            _personalDataService = personalDataService;
            _deduplicationRepository = deduplicationRepository;
        }

        //From CashinDetector
        [UsedImplicitly]
        public async Task Handle(BlockchainCashinDetector.Contract.Events.CashinCompletedEvent evt, ICommandSender commandSender)
        {
            await SendCashinEmailAsync(evt.OperationId, evt.ClientId, evt.Amount, evt.AssetId, commandSender);
        }

        //From CashoutProcessor
        [UsedImplicitly]
        public async Task Handle(CashinCompletedEvent evt, ICommandSender commandSender)
        {
            await SendCashinEmailAsync(evt.OperationId, evt.ClientId, evt.Amount, evt.AssetId, commandSender);
        }

        //From CashoutProcessor
        [UsedImplicitly]
        public async Task Handle(CashoutCompletedEvent evt, ICommandSender commandSender)
        {
            var clientModel = await _clientAccountClient.GetByIdAsync(evt.ClientId.ToString());
            var clientEmail = await _personalDataService.GetEmailAsync(evt.ClientId.ToString());
            EmailValidator.ValidateEmail(clientEmail, evt.ClientId);
            var operationId = evt.OperationId;
            if (await _deduplicationRepository.IsExistsAsync(operationId))
                return;

            var asset = await _cachedAssetsService.TryGetAssetAsync(evt.AssetId);

            var parameters = new
            {
                AssetId = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId,
                Amount = NumberFormatter.FormatNumber(evt.Amount, asset.Accuracy),
                ExplorerUrl = "",
                //{ "ExplorerUrl", string.Format(_blockchainSettings.ExplorerUrl, messageData.SrcBlockchainHash },
                Year = DateTime.UtcNow.Year.ToString()
            };

            commandSender.SendCommand(new SendEmailCommand
                {
                    ApplicationId = clientModel.PartnerId,
                    Template = "NoRefundOCashOutTemplate",
                    EmailAddresses = new[] { clientEmail },
                    Payload = parameters
                },
                EmailMessagesBoundedContext.Name);

            await _deduplicationRepository.InsertOrReplaceAsync(operationId);
        }

        private async Task SendCashinEmailAsync(Guid operationId, Guid clientId, decimal amount, string assetId, ICommandSender commandSender)
        {
            var clientModel = await _clientAccountClient.GetByIdAsync(clientId.ToString());
            var clientEmail = await _personalDataService.GetEmailAsync(clientId.ToString());
            EmailValidator.ValidateEmail(clientEmail, clientId);
            if (await _deduplicationRepository.IsExistsAsync(operationId))
                return;

            var asset = await _cachedAssetsService.TryGetAssetAsync(assetId);
            string amountFormatted = NumberFormatter.FormatNumber(amount, asset.Accuracy);

            var parameters = new 
            {
                AssetName = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId,
                Amount = amountFormatted,
                Year = DateTime.UtcNow.Year.ToString()
            };

            commandSender.SendCommand(
                new SendEmailCommand
                {
                    ApplicationId = clientModel.PartnerId,
                    Template = "NoRefundDepositDoneTemplate",
                    EmailAddresses = new[] { clientEmail },
                    Payload = parameters
                },
                EmailMessagesBoundedContext.Name);

            var notificationId = clientModel.NotificationsId;
            if (!string.IsNullOrEmpty(notificationId))
            {
                commandSender.SendCommand(new AssetsCreditedCommand()
                    {
                        Amount = (double) amount,
                        AssetId = assetId,
                        Message =
                            $"A deposit of {amountFormatted} {asset.DisplayId} has been completed to your trading wallet",
                        NotificationIds = new[] {notificationId}
                    },
                    PushNotificationsBoundedContext.Name);
            }

            await _deduplicationRepository.InsertOrReplaceAsync(operationId);
        }
    }
}