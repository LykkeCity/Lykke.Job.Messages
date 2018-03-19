using System;
using Lykke.Service.EmailSender;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Services.Templates
{
    [Obsolete]
    public interface IRemoteTemplateGenerator
    {
        Task<EmailMessage> GenerateAsync<T>(string partnerId, string templateName, T templateVm);
    }
}