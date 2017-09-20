using System.Threading.Tasks;
using Lykke.Service.EmailSender;

namespace Lykke.Job.Messages.Core.Services.Templates
{
    public interface IRemoteTemplateGenerator
    {
        Task<EmailMessage> GenerateAsync<T>(string partnerId, string templateName, T templateVm);
    }
}