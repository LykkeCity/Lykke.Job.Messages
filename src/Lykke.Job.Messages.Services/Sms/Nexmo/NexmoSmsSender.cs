using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Core.Domain.Sms;
using Lykke.Job.Messages.Core.Services.Sms;

namespace Lykke.Job.Messages.Services.Sms.Nexmo
{
    //https://docs.nexmo.com/messaging/sms-api/api-reference#keys

    public class NexmoSmsSender : ISmsSender
    {
        private readonly AppSettings.NexmoSettings _settings;
        private readonly ILog _log;

        private const string NexmoSendSmsUrlFormat = "https://rest.nexmo.com/sms/json?api_key={0}&api_secret={1}&from={2}&to={3}&text={4}";

        public NexmoSmsSender(AppSettings.NexmoSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        public string GetSenderNumber(string recipientNumber)
        {
            return recipientNumber.IsUSCanadaNumber() ? _settings.UsCanadaSender : _settings.DefaultSender;
        }

        public async Task ProcessSmsAsync(string phoneNumber, SmsMessage message)
        {
            var urlEncodedText = message.Text.EncodeUrl();
            var url = string.Format(NexmoSendSmsUrlFormat, _settings.NexmoAppKey, _settings.NexmoAppSecret, message.From, phoneNumber, urlEncodedText);
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            HttpContent responseContent = response.Content;
            NexmoResponse responseObj = null;
            string responseString;

            using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            {
                responseString = await reader.ReadToEndAsync();
                responseObj = responseString.DeserializeJson<NexmoResponse>();
            }

            if (responseObj != null)
            {
                foreach (var msg in responseObj.Messages)
                {
                    if (msg.Status != NexmoStatusCode.Success)
                    {
                        await _log.WriteWarningAsync("NexmoSMS", "ProcessSms", responseString, "SMS was not sent", DateTime.UtcNow);
                    }
                }
            }
        }
    }
}