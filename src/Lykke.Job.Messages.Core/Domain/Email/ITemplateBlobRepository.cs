using Lykke.Service.EmailSender;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.Email
{
    public interface ITemplateBlobRepository
    {
        Task<bool> CheckEmailTemplateExistsAsync(string partnerId, string templateName, string language = "EN");
        Task<EmailMessage> GetEmailTemplateAsync(string partner, string templateName, string language = "EN");
    }
}
