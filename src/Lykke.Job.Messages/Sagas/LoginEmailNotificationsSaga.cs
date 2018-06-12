using System;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Utils;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Lykke.Service.Registration.Contract.Events;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Sagas
{
    public class LoginEmailNotificationsSaga
    {        
        private readonly IPersonalDataService _personalDataService;
        private readonly IClientAccountClient _clientAccountClient;

        public LoginEmailNotificationsSaga(IPersonalDataService personalDataService, IClientAccountClient clientAccountClient)
        {            
            _personalDataService = personalDataService;
            _clientAccountClient = clientAccountClient;
        }

        [UsedImplicitly]
        public async Task Handle(ClientLoggedEvent evt, ICommandSender commandSender)
        {
            var personalData = await _personalDataService.GetAsync(evt.ClientId);
            EmailValidator.ValidateEmail(personalData.Email, evt.ClientId);

            var parameters = new 
            {
                personalData.FullName,
                evt.ClientInfo,
                evt.Ip,
                evt.Country,
                evt.City,
                Date = DateTime.UtcNow.ToString("MMMM dd, yyyy, hh:mm tt", CultureInfo.CreateSpecificCulture("en-US")),
                Year = DateTime.UtcNow.Year.ToString()
            };
            var clientAccount = await _clientAccountClient.GetByIdAsync(personalData.Id);
            var template = clientAccount.ICyprusClient
                ? "LoginNotificationCyp"
                : "LoginNotification";


            commandSender.SendCommand(
                new SendEmailCommand
                {
                    ApplicationId = evt.PartnerId,
                    Template = template,
                    EmailAddresses = new[] { personalData.Email },
                    Payload = parameters
                }, 
                EmailMessagesBoundedContext.Name);
        }

        
    }
}