using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Sms;

namespace Lykke.Job.Messages.Core.Services.Sms
{
    public interface ISmsSender
    {
        string GetSenderNumber(string recipientNumber);
        Task ProcessSmsAsync(string phoneNumber, SmsMessage message);
    }
}