namespace Lykke.Job.Messages.Contract.Email
{
    public class SendEmailData<T>
    {
        public string EmailAddress { get; set; }
        public T MessageData { get; set; }


        public static SendEmailData<T> Create(string emailAddress, T msgData)
        {
            return new SendEmailData<T>
            {
                EmailAddress = emailAddress,
                MessageData = msgData
            };
        }
    }
}