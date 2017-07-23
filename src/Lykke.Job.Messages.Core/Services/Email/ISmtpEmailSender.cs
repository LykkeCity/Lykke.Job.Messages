using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Email;

namespace Lykke.Job.Messages.Core.Services.Email
{
    public interface ISmtpEmailSender
    {
        Task SendEmailAsync(string emailAddress, EmailMessage message, string sender = null);
        Task SendBroadcastAsync(BroadcastGroup broadcastGroup, EmailMessage message);
    }
}