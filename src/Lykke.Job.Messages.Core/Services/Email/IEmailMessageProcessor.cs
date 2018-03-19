using Lykke.Job.Messages.Core.Domain.Email.Models;
using System;
using System.Threading.Tasks;
using Lykke.Messages.Email.MessageData;

namespace Lykke.Job.Messages.Core.Services.Email
{
    public interface IEmailMessageProcessor
    {
        Task SendAsync<T>(SendEmailRequest<T> Data) where T: IEmailMessageData;

        Type GetTypeForTemplateId(string templateId);
    }
}
