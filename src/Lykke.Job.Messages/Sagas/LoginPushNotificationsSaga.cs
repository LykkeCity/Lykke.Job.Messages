using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Events;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.PushNotifications.Contract.Commands;
using System;
using System.Threading.Tasks;
using UAParser;

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
            var dateTimeNow = DateTime.UtcNow;
            var notificationIds = new[] { (await _clientAccountClient.GetByIdAsync(evt.ClientId)).NotificationsId };

            var isMobile = !string.IsNullOrWhiteSpace(evt.ClientInfo);
            var devicePart = isMobile ? $"on mobile ({GetDeviceName(evt.UserAgent)})" : $"on the web ({GetBrowserName(evt.UserAgent)})";
            
            var command = new TextNotificationCommand
            {
                NotificationIds = notificationIds,
                Message = $"Successful login {devicePart} on {dateTimeNow:dd.MM.yyyy} at {dateTimeNow:HH:mm} (GMT)",
                Type = "Info"
            };

            commandSender.SendCommand(command, "push-notifications");
        }

        private string GetDeviceName(string clientInfo)
        {
            // TODO: piece of smell
            var clientData = Parser.GetDefault().Parse(clientInfo);

            if (clientData.OS.Family == "Android")
                return "Android";

            return clientData.Device.Model;
        }

        private string GetBrowserName(string browserName)
        {
            var browserData = Parser.GetDefault().Parse(browserName);

            return browserData?.UA?.Family;
        }
    }
}