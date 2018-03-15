using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.EmailSender;

namespace Lykke.Job.Messages.Core.Services.Email
{
    public interface ITemplateFormatter
    {
        Task<EmailMessage> Format(string caseId, string partnerId, string language, Dictionary<string, string> parameters);
    }
}