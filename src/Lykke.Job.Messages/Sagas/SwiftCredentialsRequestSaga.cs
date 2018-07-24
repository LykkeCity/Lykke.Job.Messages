using System;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Contract.Commands;
using Lykke.Job.Messages.Core.Services.SwiftCredentials;
using Lykke.Service.SwiftCredentials.Contracts;

namespace Lykke.Job.Messages.Sagas
{
    public class SwiftCredentialsRequestSaga
    {
        public SwiftCredentialsRequestSaga()
        {
        }
        
        [UsedImplicitly]
        public async Task Handle(SwiftCredentialsRequestedEvent evt, ICommandSender commandSender)
        {
            var templateVm = new
            {
                AssetId = evt.AssetId,
                AssetSymbol = evt.AssetSymbol,
                ClientName = evt.ClientName,
                Amount = evt.Amount,
                Year = DateTime.UtcNow.Year.ToString(),
                AccountName = evt.AccountName,
                AccountNumber = evt.AccountNumber,
                Bic = evt.Bic,
                PurposeOfPayment = evt.PurposeOfPaymentTemplate,
                BankAddress = evt.BankAddress,
                CompanyAddress = evt.CompanyAddress,
                CorrespondentAccount = evt.CorrespondentAccount
            };
            
            commandSender.SendCommand(new SendEmailCommand
                {
                    EmailAddress = evt.Email,
                    PartnerId = evt.PartnerId,
                    EmailTemplateId = "BankCashInTemplate",
                    SerializedMessageData = templateVm.ToJson()
                },
                EmailMessagesBoundedContext.Name);
        }
    }
}