namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class SwapRefundData : BaseRefundData, IEmailMessageData
    {
        public const string QueueName = "SwapRefundEmail";
    }
}