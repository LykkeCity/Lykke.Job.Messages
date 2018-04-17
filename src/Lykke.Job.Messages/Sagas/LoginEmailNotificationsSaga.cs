using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Events;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PersonalData.Contract;

namespace Lykke.Job.Messages.Sagas
{
    public class LoginEmailNotificationsSaga
    {        
        private readonly IPersonalDataService _personalDataService;

        public LoginEmailNotificationsSaga(IPersonalDataService personalDataService)
        {            
            _personalDataService = personalDataService;            
        }

        [UsedImplicitly]
        public async Task Handle(ClientLoggedEvent evt, ICommandSender commandSender)
        {
            var personalData = await _personalDataService.GetAsync(evt.ClientId);

            var parameters = new 
            {
                personalData.FullName,
                evt.ClientInfo,
                evt.Ip,
                Date = DateTime.UtcNow.ToString("MMMM dd, yyyy, hh:mm tt", CultureInfo.CreateSpecificCulture("en-US")),
                Year = DateTime.UtcNow.Year.ToString()
            };
            
            commandSender.SendCommand(
                new SendEmailCommand
                {
                    ApplicationId = evt.PartnerId,
                    Template = "LoginNotification",
                    EmailAddresses = new[] { evt.Email },
                    Payload = parameters
                }, 
                EmailMessagesBoundedContext.Name);
        }
    }
}