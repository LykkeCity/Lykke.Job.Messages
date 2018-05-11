using Common;
using Common.Log;
using Lykke.Job.Messages.Core.Domain.Email;
using Lykke.Job.Messages.Core.Domain.Email.Models;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Service.EmailSender;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Services.Email
{
    public class EmailTemplateProvider : IEmailTemplateProvide
    {
        private static readonly Regex ParameterRegex = new Regex(@"@\[([^\]]+)\]");
        private readonly ITemplateBlobRepository _templateBlobRepository;
        private readonly ILog _log;

        public EmailTemplateProvider(ITemplateBlobRepository templateBlobRepository, ILog log)
        {
            _templateBlobRepository = templateBlobRepository;
            _log = log;
        }

        public async Task<FormattedEmail> FormatAsync(string templateId, string partnerId, string language, Dictionary<string, string> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, string>();

            try
            {
                var existsForPartner = await _templateBlobRepository.CheckEmailTemplateExistsAsync(partnerId, templateId, language);
                partnerId = !existsForPartner ? "Lykke" : partnerId;
                var template = await _templateBlobRepository.GetEmailTemplateAsync(partnerId, templateId, language);

                if (template == null)
                    throw new InvalidOperationException($"Unable to find email template {templateId} ({language}) for partner {partnerId}");

                string MatchEvaluator(Match match)
                {
                    var key = match.Groups[1].Value;
                    if (parameters != null && parameters.ContainsKey(key))
                        return parameters[key];
                    throw new KeyNotFoundException($"Unable to find parameter {key} required by email template {templateId} ({language}) for partner {partnerId}");
                }

                return new FormattedEmail
                {
                    EmailMessage = new EmailMessage
                    {
                        Subject = string.IsNullOrWhiteSpace(template.Subject)
                            ? "testSubject"
                            : ParameterRegex.Replace(template.Subject, MatchEvaluator),
                        TextBody = string.IsNullOrWhiteSpace(template.TextBody)
                            ? null
                            : ParameterRegex.Replace(template.TextBody, MatchEvaluator),
                        HtmlBody = string.IsNullOrWhiteSpace(template.HtmlBody)
                            ? null
                            : ParameterRegex.Replace(template.HtmlBody, MatchEvaluator)
                    }
                };
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(EmailTemplateProvider), nameof(FormatAsync), parameters.ToJson(), ex, DateTime.UtcNow);
                throw;
            }
        }
    }
}
