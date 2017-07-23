using System;
using System.Text;
using Lykke.Job.Messages.Core.Domain.Kyc;
using Lykke.Job.Messages.Core.Extensions;

namespace Lykke.Job.Messages.Services.Email.TemplateModels
{
    public class DeclinedDocumentsTemplate
    {
        public string FullName { get; set; }
        public KycDocument[] Documents { get; set; }
        public int Year { get; set; }

        public string DocumentsAsHtml
        {
            get
            {
                var sb = new StringBuilder();

                foreach (var document in Documents)
                {
                    KycDocumentTypeApi kycDocType;
                    Enum.TryParse(document.Type, out kycDocType);

                    sb.AppendLine("<tr style='border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;'>");
                    sb.AppendLine($"<td style='padding: 15px 0 15px 0;' width='260'><span style='font-size: 1.1em;color: #8C94A0;'>{KycDocumentTypes.GetDocumentTypeName(kycDocType)}</span></td>");
                    sb.AppendLine($"<td style='padding: 15px 0 15px 0;' width='260'><span style='font-size: 1.1em;color: #3F4D60;'>{document.KycComment.HtmlBreaks()}</span></td>");
                    sb.AppendLine("</tr>");
                }

                return sb.ToString();
            }
        }
    }
}
