using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Core.Domain.Email.Models
{
    public class EmailMetada
    {
        [JsonRequired]
        [JsonProperty("subject")]
        public string Subject { get; set; }
    }
}
