using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Lykke.Service.Registration.Contract.Events;
using Lykke.SettingsReader;

namespace Lykke.Job.Messages.Sagas
{
    public class LoginEmailNotificationsSaga
    {        
        private readonly IPersonalDataService _personalDataService;
        private readonly IReloadingManager<AppSettings.CifLicenseActivationSettings> _cifLicenseActivationSettings;

        public LoginEmailNotificationsSaga(IPersonalDataService personalDataService, IReloadingManager<AppSettings.CifLicenseActivationSettings> cifLicenseActivationSettings)
        {            
            _personalDataService = personalDataService;
            _cifLicenseActivationSettings = cifLicenseActivationSettings;
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
                evt.Country,
                evt.City,
                Date = DateTime.UtcNow.ToString("MMMM dd, yyyy, hh:mm tt", CultureInfo.CreateSpecificCulture("en-US")),
                Year = DateTime.UtcNow.Year.ToString()
            };

            var template = IsCyprusClient(personalData)
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

        public bool IsCyprusClient(IPersonalData personalData)
        {
            return (personalData.MarginRegulator == _cifLicenseActivationSettings.CurrentValue.MarginRegulatorId);
        }
    }
}