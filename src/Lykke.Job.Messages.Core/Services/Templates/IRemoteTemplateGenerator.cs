using Lykke.Service.EmailSender;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Services.Templates
{
    public interface IRemoteTemplateGenerator
    {
        Task<EmailMessage> GenerateAsync<T>(string partnerId, string templateName, T templateVm);
    }
}