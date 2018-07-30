using System;
using System.Globalization;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core.Domain.DepositRefId;
using Lykke.Job.Messages.Core.Services.SwiftCredentials;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.SwiftCredentials.Contracts;

namespace Lykke.Job.Messages.Sagas
{
    public class SwiftCredentialsRequestSaga
    {
        [UsedImplicitly]
        public async Task Handle(SwiftCredentialsRequestedEvent evt, ICommandSender commandSender)
        {
            var template = new
            {
                AssetId = evt.AssetId,
                AssetSymbol = evt.AssetSymbol,
                ClientName = evt.ClientName,
                Amount = evt.Amount,
                Year = evt.Year,
                AccountName = evt.AccountName,
                AccountNumber = evt.AccountNumber,
                Bic = evt.Bic,
                PurposeOfPayment = evt.PurposeOfPayment,
                BankAddress = evt.BankAddress,
                CompanyAddress = evt.CompanyAddress,
                CorrespondentAccount = evt.CorrespondentAccount
            };
            
            commandSender.SendCommand(new SendEmailCommand
                {
                    EmailAddresses = new[] {evt.Email},
                    Template = "BankCashInTemplate",
                    Payload = template
                },
                EmailMessagesBoundedContext.Name);
        }
    }
}