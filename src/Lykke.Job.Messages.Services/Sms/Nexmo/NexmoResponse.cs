using System.Collections.Generic;

namespace Lykke.Job.Messages.Services.Sms.Nexmo
{
    public class NexmoResponse
    {
        public class NexmoMessage
        {
            public NexmoStatusCode Status { get; set; }
            public string MessageId { get; set; }
            public string To { get; set; }
            public string ClientRef { get; set; }
            public string RemainingBalance { get; set; }
            public string MessagePrice { get; set; }
            public string Network { get; set; }
            public string ErrorText { get; set; }
        }

        public int MessageCount { get; set; }
        public IEnumerable<NexmoMessage> Messages { get; set; }
    }
}