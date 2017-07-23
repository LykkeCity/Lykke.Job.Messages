using System.IO;

namespace Lykke.Job.Messages.Core.Domain.Email
{
    public class EmailAttachment
    {
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public Stream Stream { get; set; }
    }
}