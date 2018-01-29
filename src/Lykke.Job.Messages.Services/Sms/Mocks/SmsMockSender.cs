using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Sms;
using Lykke.Service.SmsSender.Client;

namespace Lykke.Job.Messages.Services.Sms.Mocks
{
    public class SmsMockSender : ISmsSenderClient
    {
        private readonly ISmsMockRepository _smsMockRepository;

        public SmsMockSender(ISmsMockRepository smsMockRepository)
        {
            _smsMockRepository = smsMockRepository;
        }

        public async Task SendSmsAsync(string phone, string message)
        {
            await _smsMockRepository.InsertAsync(phone, new SmsMessage{Text = message, From = "SMS mock sender"});
        }
    }
}
