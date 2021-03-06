﻿namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class DeclinedDocumentsData : IEmailMessageData
    {
        public const string EmailTemplateId = "DeclinedDocuments";

        public string FullName { get; set; }
        public KycDocumentData[] Documents { get; set; }
    }
}
