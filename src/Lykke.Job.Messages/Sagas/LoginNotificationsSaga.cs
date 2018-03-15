using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Commands;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Events;
using Lykke.Job.Messages.Modules;
using Lykke.Service.PersonalData.Contract;

namespace Lykke.Job.Messages.Sagas
{
    public class LoginNotificationsSaga
    {
        private readonly ITemplateFormatter _templateFormatter;
        private readonly IPersonalDataService _personalDataService;

        public LoginNotificationsSaga(ITemplateFormatter templateFormatter, IPersonalDataService personalDataService)
        {
            _templateFormatter = templateFormatter;
            _personalDataService = personalDataService;
        }

        [UsedImplicitly]
        public async Task Handle(ClientLoggedEvent evt, ICommandSender commandSender)
        {
            var partnerId = evt.PartnerId ?? "Lykke";

            var personalData = await _personalDataService.GetAsync(evt.ClientId);

            var parameters = new Dictionary<string, string>()
            {
                { "FullName", personalData.FullName },
                { "ClientInfo", evt.ClientInfo },
                { "Ip", evt.Ip }
            };

            var message = await _templateFormatter.Format("LoginNotification", partnerId, "EN", parameters);
            
            commandSender.SendCommand(new SendEmailCommand { PartnerId = evt.PartnerId, EmailAddress = evt.Email, Message = message }, "email");
        }
    }
}