﻿namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class CashInData : IEmailMessageData
    {
        public const string EmailTemplateId = "CashInEmail";

        public string Multisig { get; set; }
        public string AssetId { get; set; }
    }
}