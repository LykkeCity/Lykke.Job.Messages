using Lykke.Job.Messages.Core.Domain.Email;

namespace Lykke.Job.Messages.Contract.Email
{
    public class SendBroadcastData<T>
    {
        public BroadcastGroup BroadcastGroup { get; set; }
        public T MessageData { get; set; }


        public static SendBroadcastData<T> Create(BroadcastGroup broadcastGroup, T msgData)
        {
            return new SendBroadcastData<T>
            {
                BroadcastGroup = broadcastGroup,
                MessageData = msgData
            };
        }
    }
}