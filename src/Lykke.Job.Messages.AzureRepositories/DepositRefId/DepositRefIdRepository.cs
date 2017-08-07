using AzureStorage;
using Lykke.Job.Messages.Core.Domain.DepositRefId;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.AzureRepositories.DepositRefId
{
    public class DepositRefIdRepository : IDepositRefIdRepository
    {
        private readonly INoSQLTableStorage<DepositRefIdEntity> _tableStorage;

        public DepositRefIdRepository(INoSQLTableStorage<DepositRefIdEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async void AddCodeAsync(string refCode, string clientId, string date, string code)
        {
            DepositRefIdEntity depositRefId = new DepositRefIdEntity();
            depositRefId.ClientId = clientId;
            depositRefId.Date = date;
            depositRefId.Code = code;
            depositRefId.PartitionKey = GeneratePartitionKey(refCode);
            depositRefId.RowKey = GenerateRowKey(clientId);

            await _tableStorage.InsertAsync(new List<DepositRefIdEntity>() { depositRefId });
        }

        public static string GeneratePartitionKey(string refCode)
        {
            return refCode;
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }

    }
}
