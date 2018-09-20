using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.SmsSender.Client;
using Lykke.Service.TemplateFormatter.Client;
using Lykke.Service.TemplateFormatter.TemplateModels;

namespace Lykke.Job.Messages.Sagas
{
    public class KycSmsNotificationsSaga
    {
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IPersonalDataService _personalDataService;
        private readonly ISmsSenderClient _smsSenderClient;
        private readonly ITemplateFormatter _templateFormatter;

        public KycSmsNotificationsSaga(
            [NotNull] IClientAccountClient clientAccountClient,
            [NotNull] IPersonalDataService personalDataService,
            [NotNull] ISmsSenderClient smsSenderClient,
            [NotNull] ITemplateFormatter templateFormatter)
        {
            _clientAccountClient = clientAccountClient ?? throw new ArgumentNullException(nameof(clientAccountClient));
            _personalDataService = personalDataService ?? throw new ArgumentNullException(nameof(personalDataService));
            _smsSenderClient = smsSenderClient ?? throw new ArgumentNullException(nameof(smsSenderClient));
            _templateFormatter = templateFormatter ?? throw new ArgumentNullException(nameof(templateFormatter));
        }

        [UsedImplicitly]
        public async Task Handle(ChangeStatusEvent evt, ICommandSender commandSender)
        {
            switch (evt.NewStatus)
            {
                case nameof(KycStatus.Ok):
                    await SendSms<SmsKycApprovedTemplate>(evt.ClientId);
                    break;

                case nameof(KycStatus.NeedToFillData):
                    await SendSms<SmsKycAttentionNeededTemplate>(evt.ClientId);
                    break;
            }
        }

        private async Task SendSms<TTemplate>(string clientId) where TTemplate : new()
        {
            var clientAccount = await _clientAccountClient.GetByIdAsync(clientId);
            var personalData = await _personalDataService.GetAsync(clientId);
            var message = await _templateFormatter.FormatAsync(typeof(TTemplate).Name, clientAccount.PartnerId, "EN", new TTemplate());
            await _smsSenderClient.SendSmsAsync(personalData.ContactPhone, message.Subject);
        }
    }
}
