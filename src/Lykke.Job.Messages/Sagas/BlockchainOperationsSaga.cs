using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainCashoutProcessor.Contract.Events;
using Lykke.Job.Messages.Commands;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Resources;
using Lykke.Job.Messages.Utils;
using Lykke.Job.Messages.Workflow;
using Lykke.Service.ClientAccount.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client;

namespace Lykke.Job.Messages.Sagas
{
    //Listens On ME Rabbit
    public class BlockchainOperationsSaga
    {
        private readonly IEmailTemplateProvider _templateFormatter;
        private readonly IAssetsServiceWithCache _cachedAssetsService;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IIndex<Enum, ICqrsEngine> _engineFactory;

        public BlockchainOperationsSaga(IEmailTemplateProvider templateFormatter,
            IAssetsServiceWithCache cachedAssetsService,
            IClientAccountClient clientAccountClient,
            IIndex<Enum, ICqrsEngine> engineFactory)
        {
            _templateFormatter = templateFormatter;
            _cachedAssetsService = cachedAssetsService;
            _clientAccountClient = clientAccountClient;
            _engineFactory = engineFactory;
        }

        //From CashinDetector
        [UsedImplicitly]
        public async Task Handle(Lykke.Job.BlockchainCashinDetector.Contract.Events.CashinCompletedEvent evt)
        {
            await SendCashinEmailAsync(evt.ClientId, evt.Amount, evt.AssetId);
        }

        //From CashoutProcessor
        [UsedImplicitly]
        public async Task Handle(CashinCompletedEvent evt)
        {
            await SendCashinEmailAsync(evt.ClientId, evt.Amount, evt.AssetId);
        }

        //From CashoutProcessor
        [UsedImplicitly]
        public async Task Handle(CashoutCompletedEvent evt)
        {
            var clientModel = await _clientAccountClient.GetByIdAsync(evt.ClientId.ToString());
            var partnerId = clientModel.PartnerId ?? "Lykke";
            var asset = await _cachedAssetsService.TryGetAssetAsync(evt.AssetId);

            var parameters = new Dictionary<string, string>()
            {
                { "AssetId", asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId },
                { "Amount", evt.Amount.ToString($"F{asset.Accuracy}").TrimEnd('0') },
                { "ExplorerUrl", ""},
                //{ "ExplorerUrl", string.Format(_blockchainSettings.ExplorerUrl, messageData.SrcBlockchainHash },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };
  
            var formattedEmail = await _templateFormatter.GenerateAsync(partnerId, "NoRefundOCashOutTemplate", "EN", parameters);
            var message = formattedEmail.EmailMessage;
            var cqrsEngine = CqrsEngineRetriever.GetEngine(RabbitType.Registration, _engineFactory);
            cqrsEngine.SendCommand(new SendEmailCommand { PartnerId = clientModel.PartnerId, EmailAddress = clientModel.Email, Message = message }, 
                EmailMessagesBoundedContext.Name, 
                EmailMessagesBoundedContext.Name);
        }

        private async Task SendCashinEmailAsync(Guid clientId, decimal amount, string assetId)
        {
            var clientModel = await _clientAccountClient.GetByIdAsync(clientId.ToString());
            var partnerId = clientModel.PartnerId ?? "Lykke";
            var asset = await _cachedAssetsService.TryGetAssetAsync(assetId);

            var parameters = new Dictionary<string, string>()
            {
                { "AssetName", asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId },
                { "Amount", amount.ToString($"F{asset.Accuracy}").TrimEnd('0') },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            var formattedEmail = await _templateFormatter.GenerateAsync(partnerId, "NoRefundDepositDoneTemplate", "EN", parameters);
            var message = formattedEmail.EmailMessage;
            var cqrsEngine = CqrsEngineRetriever.GetEngine(RabbitType.Registration, _engineFactory);
            cqrsEngine.SendCommand(new SendEmailCommand { PartnerId = clientModel.PartnerId, EmailAddress = clientModel.Email, Message = message },
                EmailMessagesBoundedContext.Name,
                EmailMessagesBoundedContext.Name);
        }
    }
}