using Lykke.Job.Messages.Core.Domain.Kyc;

namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class DeclinedDocumentsData : IEmailMessageData
    {
        public string FullName { get; set; }
        public KycDocument[] Documents { get; set; }

        public string MessageId()
        {
            return "DeclinedDocuments";
        }
    }
}
