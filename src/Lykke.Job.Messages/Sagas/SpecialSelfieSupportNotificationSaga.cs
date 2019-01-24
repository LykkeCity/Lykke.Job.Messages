using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Service.EmailPartnerRouter.Contracts;

namespace Lykke.Job.Messages.Sagas
{
    public class SpecialSelfieSupportNotificationSaga
    {
        private readonly string _selfieUrl;
        private readonly string _supportEmail;

        public SpecialSelfieSupportNotificationSaga(string selfieUrl, string supportEmail)
        {
            _selfieUrl = selfieUrl;
            _supportEmail = supportEmail;
        }

        [UsedImplicitly]
        public void Handle(SelfiePostedEvent evt, ICommandSender commandSender)
        {
            var parameters = new
            {
                ClientId = evt.ClientId,
                SelfieUrl = string.Format(_selfieUrl, evt.RecoveryId)
            };
            commandSender.SendCommand(new SendEmailCommand
            {
                ApplicationId = string.Empty,
                Template = "SpecialSelfieSupportNotification",
                EmailAddresses = new[] { _supportEmail },
                Payload = parameters
            }, EmailMessagesBoundedContext.Name);
        }
    }
}
