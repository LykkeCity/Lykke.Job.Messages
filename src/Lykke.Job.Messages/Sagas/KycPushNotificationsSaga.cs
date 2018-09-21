using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.PushNotifications.Contract.Commands;
using Lykke.Service.PushNotifications.Contract.Enums;

namespace Lykke.Job.Messages.Sagas
{
    public class KycPushNotificationsSaga
    {
        private readonly IClientAccountClient _clientAccountClient;

        public KycPushNotificationsSaga([NotNull] IClientAccountClient clientAccountClient)
        {
            _clientAccountClient = clientAccountClient ?? throw new ArgumentNullException(nameof(clientAccountClient));
        }

        [UsedImplicitly]
        public async Task Handle(ChangeStatusEvent evt, ICommandSender commandSender)
        {
            switch (evt.NewStatus)
            {
                case nameof(KycStatus.Ok):
                    await SendPush(commandSender, evt.ClientId,
                        NotificationType.KycSucceess.ToString(),
                        "You are approved to trade FX."
                    );
                    break;

                case nameof(KycStatus.NeedToFillData):
                    await SendPush(
                        commandSender, evt.ClientId,
                        NotificationType.KycNeedToFillDocuments.ToString(),
                        "Some of your photos have failed verification, tap to re-upload."
                    );
                    break;

                case nameof(KycStatus.RestrictedArea):
                    await SendPush(commandSender, evt.ClientId,
                        NotificationType.KycRestrictedArea.ToString(),
                        "Lykke is not allowed to onboard clients from your region at the moment. We apologise for the inconvenience."
                    );
                    break;
            }
        }

        private async Task SendPush(ICommandSender commandSender, string clientId, string type, string message)
        {
            var pushSettings = await _clientAccountClient.GetPushNotificationAsync(clientId);
            var notificationIds = new[] { (await _clientAccountClient.GetByIdAsync(clientId)).NotificationsId };

            if (pushSettings.Enabled)
            {
                var command = new TextNotificationCommand
                {
                    NotificationIds = notificationIds,
                    Type = type,
                    Message = message
                };

                commandSender.SendCommand(command, "push-notifications");
            }
        }
    }
}
