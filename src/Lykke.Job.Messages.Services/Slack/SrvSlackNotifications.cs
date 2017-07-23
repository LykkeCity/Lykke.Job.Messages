using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Services.Http;

namespace Lykke.Job.Messages.Services.Slack
{
    public class SrvSlackNotifications
    {
        private readonly AppSettings.SlackSettings _settings;
        public SrvSlackNotifications(AppSettings.SlackSettings settings)
        {
            _settings = settings;
        }

        public async Task SendNotification(string type, string message, string sender = null)
        {
            var webHookUrl = _settings.Channels.FirstOrDefault(x => x.Type == type)?.WebHookUrl;
            if (webHookUrl != null)
            {
                var text = new StringBuilder();

                if (!string.IsNullOrEmpty(_settings.Env))
                    text.AppendLine($"Environment: {_settings.Env}");

                text.AppendLine(sender != null ? $"{sender} : {message}" : message);

                await
                    new HttpRequestClient().PostRequest(new { text = text.ToString() }.ToJson(),
                        webHookUrl);
            }
        }
    }
}