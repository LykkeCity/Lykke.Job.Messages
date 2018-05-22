﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Utils;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.Registration.Contract.Events;

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
            
            commandSender.SendCommand(
                new SendEmailCommand
                {
                    ApplicationId = evt.PartnerId,
                    Template = "LoginNotification",
                    EmailAddresses = new[] { personalData.Email },
                    Payload = parameters
                }, 
                EmailMessagesBoundedContext.Name);
        }
    }
}