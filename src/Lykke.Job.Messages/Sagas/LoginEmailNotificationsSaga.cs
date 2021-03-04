using System;
using System.Globalization;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.Registration.Contract.Events;

namespace Lykke.Job.Messages.Sagas
{
    public class LoginEmailNotificationsSaga
    {
        private readonly IPersonalDataService _personalDataService;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly ILog _log;

        public LoginEmailNotificationsSaga(
            ILogFactory logFactory,
            IPersonalDataService personalDataService,
            IClientAccountClient clientAccountClient)
        {
            _log = logFactory.CreateLog(this);
            _personalDataService = personalDataService;
            _clientAccountClient = clientAccountClient;
        }

        [UsedImplicitly]
        public async Task Handle(ClientLoggedEvent evt, ICommandSender commandSender)
        {
            var personalData = await _personalDataService.GetAsync(evt.ClientId);
            if (personalData == null)
            {
                _log.Warning(nameof(ClientLoggedEvent), $"Personal data not found for ClientId = {evt.ClientId}");
                return;
            }

            var now = DateTime.UtcNow;
            var parameters = new
            {
                personalData.FullName,
                evt.ClientInfo,
                evt.Ip,
                evt.Country,
                evt.City,
                Date = now.ToString("MMMM dd, yyyy, hh:mm tt", CultureInfo.CreateSpecificCulture("en-US")),
                Year = now.Year.ToString()
            };
            var clientAccountTask = _clientAccountClient.GetByIdAsync(personalData.Id);
            var backupTask = _clientAccountClient.GetBackupAsync(personalData.Id);

            await Task.WhenAll(clientAccountTask, backupTask);

            var clientAccount = clientAccountTask.Result;
            var backup = backupTask.Result;

            var template = clientAccount.IsCyprusClient
                ? "LoginNotificationCyp"
                : "LoginNotification";
            var applicationId = clientAccount.IsCyprusClient
                ? "LykkeCyprus"
                : evt.PartnerId;

            commandSender.SendCommand(
                new SendEmailCommand
                {
                    ApplicationId = applicationId,
                    Template = template,
                    EmailAddresses = new[] { personalData.Email },
                    Payload = parameters
                },
                EmailMessagesBoundedContext.Name);

            if (!backup.BackupDone)
            {
                commandSender.SendCommand(
                    new SendEmailCommand
                    {
                        ApplicationId = applicationId,
                        Template = "RemindBackupOnLoginTemplate",
                        EmailAddresses = new[] { personalData.Email },
                        Payload = new {Year = now.Year.ToString()}
                    },
                    EmailMessagesBoundedContext.Name);
            }
        }
    }
}
