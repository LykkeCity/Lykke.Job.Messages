using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.PushNotifications.Contract.Commands;
using Lykke.Service.Session.Contracts;

namespace Lykke.Job.Messages.Sagas
{
    public class TerminalSessionsSaga
    {
        private readonly IClientAccountClient _clientAccountClient;

        public TerminalSessionsSaga(IClientAccountClient clientAccountClient)
        {
            _clientAccountClient = clientAccountClient;
        }

        [UsedImplicitly]
        public async Task Handle(TradingSessionCreatedEvent evt, ICommandSender sender)
        {
            var command = new DataNotificationCommand
            {
                NotificationIds = new [] { (await _clientAccountClient.GetByIdAsync(evt.ClientId)).NotificationsId },
                Type =  "TradingSessionCreated"
            };

            sender.SendCommand(command, "push-notifications");
        }
    }
}