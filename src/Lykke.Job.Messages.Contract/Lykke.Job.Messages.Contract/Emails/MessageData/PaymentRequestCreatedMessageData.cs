namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class PaymentRequestCreatedMessageData : IEmailMessageData
    {
        public const string EmailTemplateId = "PaymentRequestCreatedEmail";

        public string InvoiceNumber { get; set; }
        public string Company { get; set; }
        public string ClientFullName { get; set; }
        public decimal AmountToBePaid { get; set; }
        public string SettlementCurrency { get; set; }
        public string DueDate { get; set; }
        public string CheckoutLink { get; set; }
        public string Note { get; set; }
        public int Year { get; set; }
    }
}