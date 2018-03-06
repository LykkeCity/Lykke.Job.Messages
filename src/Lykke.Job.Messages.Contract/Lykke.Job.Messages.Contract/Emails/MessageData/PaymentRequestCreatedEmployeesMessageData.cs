namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class PaymentRequestCreatedEmployeesMessageData : IEmailMessageData
    {
        public const string QueueName = "PaymentRequestCreatedEmployeesEmail";

        public string InvoiceNumber { get; set; }
        public string ClientFullName { get; set; }
        public decimal AmountToBePaid { get; set; }
        public string SettlementCurrency { get; set; }
        public string DueDate { get; set; }
        public string InvoiceDetailsLink { get; set; }
        public int Year { get; set; }
    }
}