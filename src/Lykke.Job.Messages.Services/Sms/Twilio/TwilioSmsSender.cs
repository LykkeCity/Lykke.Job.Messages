using System.Threading.Tasks;
using Common;
using Common.Log;
using Common.PasswordTools;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Core.Domain.Sms;
using Lykke.Job.Messages.Core.Services.Sms;

namespace Lykke.Job.Messages.Services.Sms.Twilio
{
    public class TwilioSmsSender : IAlternativeSmsSender
    {
        private readonly ILog _log;
        private readonly AppSettings.TwilioSettings _twilioSettings;
        private readonly TwilioRestClient _twilioRestClient;

        public TwilioSmsSender(AppSettings.TwilioSettings settings, ILog log)
        {
            _log = log;
            _twilioSettings = settings;
            _twilioRestClient = new TwilioRestClient(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
        }

        public string GetSenderNumber(string recipientNumber)
        {
            return recipientNumber.IsUSCanadaNumber() ? _twilioSettings.UsSender : _twilioSettings.SwissSender;
        }

        public async Task ProcessSmsAsync(string phoneNumber, SmsMessage message)
        {
            var msg = await _twilioRestClient.SendMessage(message.From, phoneNumber, message.Text);

            if (!msg.Success)
                await _log.WriteWarningAsync("TwilioSmsSender", "ProcessSmsAsync", PasswordKeepingUtils.GetClientHashedPwd(phoneNumber), msg.ErrorMesssage);
        }
    }
}