using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Core;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.Kyc.Abstractions.Domain.Documents;
using Lykke.Service.Kyc.Abstractions.Domain.Documents.Types;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.TemplateFormatter.TemplateModels;

namespace Lykke.Job.Messages.Sagas
{
    public class KycEmailNotificationsSaga
    {
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IPersonalDataService _personalDataService;
        private readonly IKycDocumentsServiceV2 _kycDocumentsService;
        private readonly LykkeKycWebsiteUrlSettings _websiteUrlSettings;

        public KycEmailNotificationsSaga(
            [NotNull] IClientAccountClient clientAccountClient,
            [NotNull] IPersonalDataService personalDataService,
            [NotNull] IKycDocumentsServiceV2 kycDocumentsService,
            [NotNull] LykkeKycWebsiteUrlSettings websiteUrlSettings)
        {
            _clientAccountClient = clientAccountClient ?? throw new ArgumentNullException(nameof(clientAccountClient));
            _personalDataService = personalDataService ?? throw new ArgumentNullException(nameof(personalDataService));
            _kycDocumentsService = kycDocumentsService ?? throw new ArgumentNullException(nameof(kycDocumentsService));
            _websiteUrlSettings = websiteUrlSettings ?? throw new ArgumentNullException(nameof(websiteUrlSettings));
        }

        [UsedImplicitly]
        public async Task Handle(ChangeStatusEvent evt, ICommandSender commandSender)
        {
            var clientAccount = await _clientAccountClient.GetByIdAsync(evt.ClientId);
            var personalData = await _personalDataService.GetAsync(evt.ClientId);
            var applicationId = clientAccount.IsCyprusClient ? "LykkeCyprus" : clientAccount.PartnerId;

            var baseTemplateData = new BaseTemplate
            {
                Year = DateTime.UtcNow.Year.ToString()
            };

            switch (evt.NewStatus)
            {
                case nameof(KycStatus.Ok):
                    var welcomeTemplate = clientAccount.IsCyprusClient ? "WelcomeFxCypTemplate" : "WelcomeFxTemplate";
                    await SendEmail(commandSender, evt.ClientId, applicationId, welcomeTemplate, baseTemplateData);
                    break;

                case nameof(KycStatus.NeedToFillData):
                    var declinedDocumentsData = await GetDeclinedDocumentsData(evt.ClientId, personalData.FullName, _websiteUrlSettings.Url);
                    await SendEmail(commandSender, evt.ClientId, applicationId, "DeclinedDocumentsTemplate", declinedDocumentsData);
                    break;

                case nameof(KycStatus.Rejected):
                    var rejectTemplate = clientAccount.IsCyprusClient ? "RejectedCypTemplate" : "RejectedTemplate";
                    await SendEmail(commandSender, evt.ClientId, applicationId, rejectTemplate, baseTemplateData);
                    break;

                case nameof(KycStatus.RestrictedArea):
                    var restrictedData = new RestrictedAreaTemplate {
                        FirstName = personalData.FirstName,
                        LastName = personalData.LastName,
                        Year = DateTime.UtcNow.Year
                    };
                    await SendEmail(commandSender, evt.ClientId, applicationId, "RestrictedAreaTemplate", restrictedData);
                    break;
            }
        }

        private async Task SendEmail(ICommandSender commandSender, string clientId, string applicationId, string template, object message)
        {
            if (message == null)
            {
                return;
            }

            var email = await _personalDataService.GetEmailAsync(clientId);

            commandSender.SendCommand(new SendEmailCommand {
                ApplicationId = applicationId,
                Template = template,
                EmailAddresses = new[] { email },
                Payload = message
            },
            EmailMessagesBoundedContext.Name);
        }

        private async Task<DeclinedDocumentsTemplate> GetDeclinedDocumentsData(string clientId, string fullName, string webUrl)
        {
            var documents = await _kycDocumentsService.GetCurrentDocumentsAsync(clientId);
            var declinedDocuments = documents
                .Where(item => item.Status.Name == CheckDocumentPorcess.DeclinedState.Name && item.Type.Name != OtherDocument.ApiType)
                .ToArray();

            if (declinedDocuments.Length == 0)
            {
                return null;
            }

            var documentsAsHtml = new StringBuilder();
            foreach (var document in declinedDocuments)
            {
                string kycDocType = document.Type.Name;
                switch (document.Type.Name.ToLower())
                {
                    case "idcard":
                        kycDocType = "Passport or ID";
                        break;
                    case "idcardbackside":
                        kycDocType = "Passport or ID (back side)";
                        break;
                    case "proofofaddress":
                        kycDocType = "Proof of address";
                        break;
                }

                var comment = document.Status.Properties?["Reason"]?.ToObject<string>() ?? string.Empty;

                documentsAsHtml.AppendLine(
                    "<tr style='border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;'>");
                documentsAsHtml.AppendLine(
                    $"<td style='padding: 15px 0 15px 0;' width='260'><span style='font-size: 1.1em;color: #8C94A0;'>{kycDocType}</span></td>");
                documentsAsHtml.AppendLine(
                    $"<td style='padding: 15px 0 15px 0;' width='260'><span style='font-size: 1.1em;color: #3F4D60;'>{comment.Replace("\r\n", "<br>")}</span></td>");
                documentsAsHtml.AppendLine("</tr>");
            }

            return new DeclinedDocumentsTemplate
            {
                FullName = fullName,
                LykkeKycWebsiteUrl = webUrl,
                DocumentsAsHtml = documentsAsHtml.ToString(),
                Year = DateTime.UtcNow.Year
            };
        }
    }
}
