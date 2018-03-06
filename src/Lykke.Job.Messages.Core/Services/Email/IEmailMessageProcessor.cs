using Lykke.Job.Messages.Contract.Emails.MessageData;
using Lykke.Job.Messages.Core.Domain.Email.Models;
using System;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Services.Email
{
    public interface IEmailMessageProcessor
    {
        Task SendAsync<T>(SendEmailRequest<T> Data) where T: IEmailMessageData;

        Type GetTypeForTemplateId(string templateId);
    }
}
