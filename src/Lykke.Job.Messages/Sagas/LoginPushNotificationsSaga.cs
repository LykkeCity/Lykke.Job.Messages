using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.PushNotifications.Contract.Commands;
using Lykke.Service.PushNotifications.Contract.Enums;
using Lykke.Service.Registration.Contract.Events;
using Lykke.Service.TemplateFormatter.Client;
using UAParser;

namespace Lykke.Job.Messages.Sagas
{
    public class LoginPushNotificationsSaga
    {
        private readonly IClientAccountClient _clientAccountClient;
        private readonly ITemplateFormatter _templateFormatter;
        private readonly ILog _log;

        public LoginPushNotificationsSaga(
            IClientAccountClient clientAccountClient,
            ITemplateFormatter templateFormatter,
            ILogFactory logFactory
            )
        {
            _clientAccountClient = clientAccountClient;
            _templateFormatter = templateFormatter;
            _log = logFactory.CreateLog(this);
        }

        [UsedImplicitly]
        public async Task Handle(ClientLoggedEvent evt, ICommandSender commandSender)
        {            
            var dateTimeNow = DateTime.UtcNow;
            var clientAccount = await _clientAccountClient.GetByIdAsync(evt.ClientId);

            if (clientAccount == null)
            {
                _log.Warning(nameof(ClientLoggedEvent), $"Client not found (clientId = {evt.ClientId})");
                return;
            }
            
            var pushSettings = await _clientAccountClient.GetPushNotificationAsync(evt.ClientId);

            if (!pushSettings.Enabled || string.IsNullOrEmpty(clientAccount.NotificationsId))
                return;
            
            var isMobile = !string.IsNullOrWhiteSpace(evt.ClientInfo);
            
            var template = await _templateFormatter.FormatAsync(
                "PushLoginSuccessfulTemplate",
                clientAccount.PartnerId,
                "EN", 
                new
                {
                    DeviceType = isMobile
                        ? "on mobile"
                        : "on the web",
                    Name = string.IsNullOrWhiteSpace(evt.UserAgent)
                        ? null
                        : (isMobile ? GetDeviceName(evt.UserAgent) : GetBrowserName(evt.UserAgent)),
                    Date = $"{dateTimeNow:dd.MM.yyyy}",
                    Time = $"{dateTimeNow:HH:mm}"
                });

            if (template != null)
            {
                commandSender.SendCommand(new TextNotificationCommand
                {
                    NotificationIds = new[] {clientAccount.NotificationsId},
                    Message = template.Subject,
                    Type = NotificationType.Info.ToString()
                }, PushNotificationsBoundedContext.Name);
            }
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
