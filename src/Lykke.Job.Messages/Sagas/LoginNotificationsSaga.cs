using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Commands;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Events;
using Lykke.Job.Messages.Utils;
using Lykke.Job.Messages.Workflow;
using Lykke.Service.PersonalData.Contract;

namespace Lykke.Job.Messages.Sagas
{
    public class LoginNotificationsSaga
    {
        private readonly IEmailTemplateProvider _templateFormatter;
        private readonly IPersonalDataService _personalDataService;
        private readonly IIndex<Enum, ICqrsEngine> _engineFactory;

        public LoginNotificationsSaga(IIndex<Enum, ICqrsEngine> engineFactory, 
            IEmailTemplateProvider templateFormatter, 
            IPersonalDataService personalDataService)
        {
            _templateFormatter = templateFormatter;
            _personalDataService = personalDataService;
            _engineFactory = engineFactory;

        }

        [UsedImplicitly]
        public async Task Handle(ClientLoggedEvent evt)
        {
            var partnerId = evt.PartnerId ?? "Lykke";

            var personalData = await _personalDataService.GetAsync(evt.ClientId);

            var parameters = new Dictionary<string, string>()
            {
                { "FullName", personalData.FullName },
                { "ClientInfo", evt.ClientInfo },
                { "Ip", evt.Ip },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            var formattedEmail = await _templateFormatter.GenerateAsync(partnerId, "LoginNotificationTemplate", "EN", parameters);
            var message = formattedEmail.EmailMessage;
            var cqrsEngine = CqrsEngineRetriever.GetEngine(RabbitType.Registration, _engineFactory);
            cqrsEngine.SendCommand(new SendEmailCommand { PartnerId = evt.PartnerId, EmailAddress = evt.Email, Message = message }, 
                EmailMessagesBoundedContext.Name, 
                EmailMessagesBoundedContext.Name);
        }
    }
}