namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class RequestForDocumentData : IEmailMessageData
    {
        public const string EmailTemplateId = "RequestForDocument";


        public string FullName { get; set; }
        public string Text { get; set; }
        public string Comment { get; set; }
        public string Year { get; set; }
    }
}
