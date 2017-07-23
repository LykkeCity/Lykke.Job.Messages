using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.Sms
{
    public interface ISmsMockRepository
    {
        Task InsertAsync(string phoneNumber, SmsMessage msg);
    }
}