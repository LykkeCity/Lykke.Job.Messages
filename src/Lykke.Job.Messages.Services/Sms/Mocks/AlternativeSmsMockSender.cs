using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Sms;
using Lykke.Job.Messages.Core.Services.Sms;

namespace Lykke.Job.Messages.Services.Sms.Mocks
{
    public class AlternativeSmsMockSender : IAlternativeSmsSender
    {
        private readonly ISmsMockRepository _smsMockRepository;

        public AlternativeSmsMockSender(ISmsMockRepository smsMockRepository)
        {
            _smsMockRepository = smsMockRepository;
        }

        public string GetSenderNumber(string recipientNumber)
        {
            return "AlternativeSmsMockSender";
        }

        public Task ProcessSmsAsync(string phoneNumber, SmsMessage message)
        {
            return _smsMockRepository.InsertAsync(phoneNumber, message);
        }
    }
}