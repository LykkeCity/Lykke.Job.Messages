using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.PushNotifications.Contract.Commands;
using Lykke.Service.PushNotifications.Contract.Enums;
using Lykke.Service.TemplateFormatter.Client;
using EmailMessage = Lykke.Service.EmailSender.EmailMessage;

namespace Lykke.Job.Messages.Sagas
{
    public class KycPushNotificationsSaga
    {
        private readonly IClientAccountClient _clientAccountClient;
        [NotNull] private readonly ITemplateFormatter _templateFormatter;
        private readonly ILog _log;

        public KycPushNotificationsSaga(
            [NotNull] IClientAccountClient clientAccountClient,
            [NotNull] ITemplateFormatter templateFormatter,
            ILogFactory logFactory
            )
        {
            _clientAccountClient = clientAccountClient ?? throw new ArgumentNullException(nameof(clientAccountClient));
            _templateFormatter = templateFormatter ?? throw new ArgumentNullException(nameof(templateFormatter));
            _log = logFactory.CreateLog(this);
        }

        [UsedImplicitly]
        public async Task Handle(ChangeStatusEvent evt, ICommandSender commandSender)
        {
            var clientAccount = await _clientAccountClient.GetClientByIdAsync(evt.ClientId);

            if (clientAccount == null)
            {
                _log.Warning(nameof(ChangeStatusEvent), $"Client not found (clientId = {evt.ClientId})");
                return;
            }
            
            var pushSettings = await _clientAccountClient.GetPushNotificationAsync(evt.ClientId);

            if (!pushSettings.Enabled || string.IsNullOrEmpty(clientAccount.NotificationsId))
                return;
            
            EmailMessage template = null;
            string type = null;
            
            switch (evt.NewStatus)
            {
                case nameof(KycStatus.Ok):
                    template = await _templateFormatter.FormatAsync("PushKycSuccessTemplate", clientAccount.PartnerId, "EN", new { });
                    type = NotificationType.KycSucceess.ToString();
                    break;

                case nameof(KycStatus.NeedToFillData):
                    template = await _templateFormatter.FormatAsync("PushKycNeedDocumentsTemplate", clientAccount.PartnerId, "EN", new { });
                    type = NotificationType.KycNeedToFillDocuments.ToString();
                    break;

                case nameof(KycStatus.RestrictedArea):
                    template = await _templateFormatter.FormatAsync("PushKycRestrictedTemplate", clientAccount.PartnerId, "EN", new { });
                    type = NotificationType.KycRestrictedArea.ToString();
                    break;
            }

            if (template == null)
                return;
            
            commandSender.SendCommand(new TextNotificationCommand
            {
                NotificationIds = new[] {clientAccount.NotificationsId},
                Type = type,
                Message = template.Subject
            }, PushNotificationsBoundedContext.Name);
        }
    }
}
