using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Events;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.PushNotifications.Contract.Commands;

namespace Lykke.Job.Messages.Sagas
{
    public class LoginPushNotificationsSaga
    {
        private readonly IClientAccountClient _clientAccountClient;

        public LoginPushNotificationsSaga(IClientAccountClient clientAccountClient)
        {
            _clientAccountClient = clientAccountClient;
        }

        [UsedImplicitly]
        public async Task Handle(ClientLoggedEvent evt, ICommandSender commandSender)
        {
            var notificationIds = new[] { (await _clientAccountClient.GetByIdAsync(evt.ClientId)).NotificationsId };

            var command = new TextNotificationCommand
            {
                NotificationIds = notificationIds,
                Message = "Successful login" + (!string.IsNullOrWhiteSpace(evt.ClientInfo) ? $", {evt.ClientInfo}": ""),
                Type = "Info"
            };

            commandSender.SendCommand(command, "push-notifications");
        }
    }
}