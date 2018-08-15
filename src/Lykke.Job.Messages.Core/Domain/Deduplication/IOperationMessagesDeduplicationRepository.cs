using System;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.Deduplication
{
    public interface IOperationMessagesDeduplicationRepository
    {
        Task InsertOrReplaceAsync(Guid operationId);
        Task<bool> IsExistsAsync(Guid operationId);
        Task TryRemoveAsync(Guid operationId);
    }
}
