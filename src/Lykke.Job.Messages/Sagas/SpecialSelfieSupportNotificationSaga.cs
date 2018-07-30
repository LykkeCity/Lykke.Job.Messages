
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.SettingsReader;

namespace Lykke.Job.Messages.Sagas
{
    public class SpecialSelfieSupportNotificationSaga
    {
        private readonly AppSettings.SpecialSelfieSetting _settings;
        public SpecialSelfieSupportNotificationSaga(AppSettings.SpecialSelfieSetting settings)
        {
            _settings = settings;
        }

        [UsedImplicitly]
        public async Task Handle(SelfiePostedEvent evt, ICommandSender commandSender)
        {
            var parameters = new
            {
                ClientId = evt.ClientId,
                SelfieUrl = string.Format(_settings.SelfieUrl, evt.RecoveryId)
            };
            commandSender.SendCommand(new SendEmailCommand
            {
                ApplicationId = string.Empty,
                Template = "SpecialSelfieSupportNotification",
                EmailAddresses = new[] { _settings.SupportEmail },
                Payload = parameters
            }, EmailMessagesBoundedContext.Name);
        }
    }
}
