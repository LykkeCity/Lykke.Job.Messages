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
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Lykke.Job.Messages.Services.Email
{
    public class EmailTemplateProvider : IEmailTemplateProvider
    {
        private static readonly Regex ParameterRegex = new Regex(@"@\[([^\]]+)\]");
        private readonly ITemplateBlobRepository _templateBlobRepository;
        private readonly ILog _log;
        private readonly IMemoryCache _cache;

        public EmailTemplateProvider(ITemplateBlobRepository templateBlobRepository, ILog log, IMemoryCache cache)
        {
            _templateBlobRepository = templateBlobRepository;
            _log = log;
            _cache = cache;
        }

        public async Task<FormattedEmail> GenerateAsync<T>(string partnerId, string templateId, string language, T templateVm)
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(templateVm)) ??
                             new Dictionary<string, string>();

            try
            {
                string memoryCacheKey = $"{partnerId}_{templateId}_{language}";
                EmailMessage template = null;
                if (!_cache.TryGetValue(memoryCacheKey, out template))
                {
                    var existsForPartner = await _templateBlobRepository.CheckEmailTemplateExistsAsync(partnerId, templateId, language);
                    partnerId = !existsForPartner ? "Lykke" : partnerId;
                    template = await _templateBlobRepository.GetEmailTemplateAsync(partnerId, templateId, language);

                    if (template == null)
                        throw new InvalidOperationException($"Unable to find email template {templateId} ({language}) for partner {partnerId}");

                    _cache.GetOrCreate(memoryCacheKey, (entry) =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                        return template;
                    });
                }

                string MatchEvaluator(Match match)
                {
                    var key = match.Groups[1].Value;
                    if (parameters.ContainsKey(key))
                        return parameters[key];

                    throw new KeyNotFoundException($"Unable to find parameter {key} required by email template {templateId} ({language}) for partner {partnerId}");
                }

                var emailMesage = new EmailMessage
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
                };

                return new FormattedEmail
                {
                    EmailMessage = emailMesage
                };
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(EmailTemplateProvider), nameof(GenerateAsync), parameters.ToJson(), ex, DateTime.UtcNow);
                throw;
            }
        }
    }
}
