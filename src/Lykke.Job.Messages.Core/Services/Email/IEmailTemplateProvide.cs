using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Email.Models;

namespace Lykke.Job.Messages.Core.Services.Email
{
    public interface IEmailTemplateProvide
    {
        Task<FormattedEmail> FormatAsync(string templateId, string partnerId, string language, Dictionary<string, string> parameters);
    }
}