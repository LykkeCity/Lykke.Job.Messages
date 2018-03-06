using Lykke.Job.Messages.Contract.Emails;
using Lykke.Job.Messages.Core.Domain.Email;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.Messages.AzureRepositories.Email
{
    public class BroadcastMailEntity : TableEntity, IBroadcastMail
    {
        public string Email => RowKey;

        public static string GeneratePartitionKey(BroadcastGroup broadcastGroup)
        {
            return ((int)broadcastGroup).ToString();
        }

        public static string GenerateRowKey(string email)
        {
            return email;
        }

        public BroadcastGroup Group
        {
            get { return (BroadcastGroup)int.Parse(PartitionKey); }
            set
            {
                PartitionKey = ((int)value).ToString();
            }
        }

        public static BroadcastMailEntity Create(IBroadcastMail broadcastMail)
        {
            return new BroadcastMailEntity
            {
                RowKey = broadcastMail.Email,
                Group = broadcastMail.Group
            };
        }
    }
}