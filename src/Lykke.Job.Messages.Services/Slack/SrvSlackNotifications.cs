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
        private readonly SlackChannel[] _slackChannels;

        public SrvSlackNotifications(SlackChannel[] slackChannels)
        {
            _slackChannels = slackChannels;
        }

        public async Task SendNotification(string type, string message, string sender = null)
        {
            var webHookUrl = _slackChannels.FirstOrDefault(x => x.Type == type)?.WebHookUrl;
            if (webHookUrl == null)
                return;

            var text = new StringBuilder();

            text.AppendLine(sender != null ? $"{sender} : {message}" : message);

            await new HttpRequestClient().PostRequest(new {text = text.ToString()}.ToJson(), webHookUrl);
        }
    }
}