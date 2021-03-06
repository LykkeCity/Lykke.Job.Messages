﻿namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class PrivateWalletAddressData : IEmailMessageData
    {
        public const string EmailTemplateId = "PrivateWalletAddressEmail";

        public string Address { get; set; }
        public string Name { get; set; }
    }
}
