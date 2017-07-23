namespace Lykke.Job.Messages.Contract.Sms
{
    public class SendSmsData<T>
    {
        public string PhoneNumber { get; set; }
        public T MessageData { get; set; }
        public bool UseAlternativeProvider { get; set; }
    }
}