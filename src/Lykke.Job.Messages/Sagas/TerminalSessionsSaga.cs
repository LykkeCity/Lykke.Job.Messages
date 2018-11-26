using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.PushNotifications.Contract.Commands;
using Lykke.Service.PushNotifications.Contract.Enums;
using Lykke.Service.Session.Contracts;

namespace Lykke.Job.Messages.Sagas
{
    public class TerminalSessionsSaga
    {
        private readonly IClientAccountClient _clientAccountClient;
        private readonly ILog _log;

        public TerminalSessionsSaga(
            IClientAccountClient clientAccountClient,
            ILogFactory logFactory)
        {
            _clientAccountClient = clientAccountClient;
            _log = logFactory.CreateLog(this);
        }

        [UsedImplicitly]
        public async Task Handle(TradingSessionCreatedEvent evt, ICommandSender commandSender)
        {
            var clientAccount = await _clientAccountClient.GetClientByIdAsync(evt.ClientId);

            if (clientAccount == null)
            {
                _log.Warning(nameof(TradingSessionCreatedEvent), $"Client not found (clientId = {evt.ClientId})");
                return;
            }
            
            var pushSettings = await _clientAccountClient.GetPushNotificationAsync(evt.ClientId);

            if (!pushSettings.Enabled || string.IsNullOrEmpty(clientAccount.NotificationsId))
                return;
            
            commandSender.SendCommand(new DataNotificationCommand
            {
                NotificationIds = new[] {clientAccount.NotificationsId},
                Type = NotificationType.TradingSessionCreated.ToString()
            }, PushNotificationsBoundedContext.Name);
        }
    }
}
