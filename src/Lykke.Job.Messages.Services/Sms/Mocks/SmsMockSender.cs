using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Sms;
using Lykke.Job.Messages.Core.Services.Sms;

namespace Lykke.Job.Messages.Services.Sms.Mocks
{
    public class SmsMockSender : ISmsSender
    {
        private readonly ISmsMockRepository _smsMockRepository;

        public SmsMockSender(ISmsMockRepository smsMockRepository)
        {
            _smsMockRepository = smsMockRepository;
        }

        public string GetSenderNumber(string recipientNumber)
        {
            return "SmsMockSender";
        }

        public Task ProcessSmsAsync(string phoneNumber, SmsMessage message)
        {
            return _smsMockRepository.InsertAsync(phoneNumber, message);
        }
    }
}