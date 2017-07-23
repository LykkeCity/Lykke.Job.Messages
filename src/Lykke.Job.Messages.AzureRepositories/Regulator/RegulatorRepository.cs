using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.Messages.Core.Regulator;

namespace Lykke.Job.Messages.AzureRepositories.Regulator
{
    public class RegulatorRepository : IRegulatorRepository
    {
        private readonly INoSQLTableStorage<RegulatorEntity> _tableStorage;

        public RegulatorRepository(INoSQLTableStorage<RegulatorEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IRegulator> GetByIdOrDefaultAsync(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var regulator = await _tableStorage.GetDataAsync(RegulatorEntity.GeneratePartition(), RegulatorEntity.GenerateRowKey(id));

                if (regulator != null)
                    return regulator;
            }

            var allRegulators = await _tableStorage.GetDataAsync(RegulatorEntity.GeneratePartition());
            var defaultRegulators = allRegulators.Where(r => r.IsDefault).ToArray();

            if (defaultRegulators.Length == 1)
                return defaultRegulators.Single();

            return null;
        }

        public Task RemoveAsync(string id)
        {
            return _tableStorage.DeleteAsync(RegulatorEntity.GeneratePartition(), RegulatorEntity.GenerateRowKey(id));
        }
    }
}