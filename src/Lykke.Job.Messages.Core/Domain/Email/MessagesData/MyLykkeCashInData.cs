namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class MyLykkeCashInData : IEmailMessageData
    {


        public string ConversionWalletAddress { get; set; }
        public double Amount { get; set; }
        public double LkkAmount { get; set; }
        public double Price { get; set; }
        public string AssetId { get; set; }


        public static MyLykkeCashInData Create(string conversionWalletAddress, double amount, double lkkAmount,
            double price,
            string assetId)
        {
            return new MyLykkeCashInData()
            {
                ConversionWalletAddress = conversionWalletAddress,
                Amount = amount,
                LkkAmount = lkkAmount,
                Price = price,
                AssetId = assetId,
            };
        }

        public string MessageId()
        {
            return "MyLykkeCashIn";
        }
    }
}
