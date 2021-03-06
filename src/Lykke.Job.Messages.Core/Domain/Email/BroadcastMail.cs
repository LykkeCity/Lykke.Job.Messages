using Lykke.Job.Messages.Contract.Emails;

namespace Lykke.Job.Messages.Core.Domain.Email
{
    public class BroadcastMail : IBroadcastMail
    {
        public string Email { get; set; }
        public BroadcastGroup Group { get; set; }
    }
}