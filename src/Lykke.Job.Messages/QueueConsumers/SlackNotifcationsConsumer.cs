using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.Messages.Contract.Slack;
using Lykke.Job.Messages.Services.Slack;
using Lykke.JobTriggers.Triggers.Attributes;

namespace Lykke.Job.Messages.QueueConsumers
{
    public class SlackNotifcationsConsumer
    {
        private readonly SrvSlackNotifications _srvSlackNotifications;
        private readonly ILog _log;

        public SlackNotifcationsConsumer(SrvSlackNotifications srvSlackNotifications, ILog log)
        {
            _srvSlackNotifications = srvSlackNotifications;
            _log = log;
        }

        //[QueueTrigger("slack-notifications")]
        public async Task ProcessInMessage(SlackNotificationRequestMsg msg)
        {
            try
            {
                await _srvSlackNotifications.SendNotification(msg.Type, msg.Message, msg.Sender);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SlackNotificationRequestsConsumer", "ProcessInMessage", msg.ToJson(), ex);
                throw;
            }
        }
    }
}