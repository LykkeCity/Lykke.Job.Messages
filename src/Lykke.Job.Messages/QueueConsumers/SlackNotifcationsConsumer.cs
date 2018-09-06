using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.Messages.Contract.Slack;
using Lykke.Job.Messages.Services.Slack;

namespace Lykke.Job.Messages.QueueConsumers
{
    public class SlackNotifcationsConsumer
    {
        private readonly SrvSlackNotifications _srvSlackNotifications;
        private readonly ILog _log;

        public SlackNotifcationsConsumer(SrvSlackNotifications srvSlackNotifications, ILogFactory logFactory)
        {
            _srvSlackNotifications = srvSlackNotifications;
            _log = logFactory.CreateLog(this);
        }

        public async Task ProcessInMessage(SlackNotificationRequestMsg msg)
        {
            try
            {
                await _srvSlackNotifications.SendNotification(msg.Type, msg.Message, msg.Sender);
            }
            catch (Exception ex)
            {
                _log.Error(nameof(ProcessInMessage), ex, msg.ToJson());
                throw;
            }
        }
    }
}
