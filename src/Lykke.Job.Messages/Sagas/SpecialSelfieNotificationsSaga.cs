using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PersonalData.Contract;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Sagas
{
    public class SpecialSelfieNotificationsSaga
    {
        private readonly IPersonalDataService _personalDataService;
        private readonly IClientAccountClient _clientAccountClient;
        public SpecialSelfieNotificationsSaga(IPersonalDataService personalDataService, IClientAccountClient clientAccountClient)
        {
            _personalDataService = personalDataService;
            _clientAccountClient = clientAccountClient;
        }

        [UsedImplicitly]
        public async Task Handle(SpecialSelfieEvent evt, ICommandSender commandSender)
        {
            var personalData = await _personalDataService.GetAsync(evt.ClientId);

            var parameters = new
            {
                personalData.FullName,
                evt.Reason
            };
            var clientAccount = await _clientAccountClient.GetByIdAsync(personalData.Id);
            var template = evt.Status == "Approved"
                ? "SpecialSelfieApproved"
                : "SpecialSelfieRejected";
            var applicationId = clientAccount.IsCyprusClient
                ? "LykkeCyprus"
                : evt.PartnerId;

            commandSender.SendCommand(
                new SendEmailCommand
                {
                    ApplicationId = applicationId,
                    Template = template,
                    EmailAddresses = new[] { personalData.Email },
                    Payload = parameters
                },
                EmailMessagesBoundedContext.Name);
        }
    }
}
