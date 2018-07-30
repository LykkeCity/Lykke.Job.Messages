using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Contract
{
    public class SpecialSelfieEvent
    {
        public string ClientId { get; set; }
        public string PartnerId { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
    }

    public class SelfiePostedEvent
    {
        public string SelfieId { get; set; }
        public string ClientId { get; set; }
        public string RecoveryId { get; set; }
    }
    public enum SelfieStatus
    {
        Approved,
        Rejected
    }
}
