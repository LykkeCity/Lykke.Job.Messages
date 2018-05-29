using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.SwiftWithdrawal.Contracts;

namespace Lykke.Job.Messages.Sagas
{
    public class SwiftWithdrawalEmailNotificationSaga
    {        
        private readonly IPersonalDataService _personalDataService;
        private readonly IClientAccountClient _clientAccountClient;

        public SwiftWithdrawalEmailNotificationSaga(IPersonalDataService personalDataService, IClientAccountClient clientAccountClient)
        {
            _personalDataService = personalDataService;
            _clientAccountClient = clientAccountClient;
        }

        [UsedImplicitly]
        public async Task Handle(SwiftCashoutCreatedEvent evt, ICommandSender commandSender)
        {
            var clientModel = await _clientAccountClient.GetByIdAsync(evt.ClientId);
            var personalData = await _personalDataService.GetAsync(evt.ClientId);
            
            var parameters = new 
            {
                personalData.FullName,
                Year = DateTime.UtcNow.Year.ToString()
            };
            
            commandSender.SendCommand(
                new SendEmailCommand
                {
                    ApplicationId = clientModel.PartnerId,
                    Template = "SwiftCashoutRequested",
                    EmailAddresses = new[] { personalData.Email },
                    Payload = parameters
                }, 
                EmailMessagesBoundedContext.Name);
        }
    }
}