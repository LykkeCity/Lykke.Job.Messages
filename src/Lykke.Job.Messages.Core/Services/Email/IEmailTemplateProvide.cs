using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Email.Models;

namespace Lykke.Job.Messages.Core.Services.Email
{
    public interface IEmailTemplateProvider
    {
        Task<FormattedEmail> GenerateAsync<T>(string partnerId, string templateId, string language, T templateVm);
    }
}