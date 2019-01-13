using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.SwiftCredentials;
using Lykke.Job.Messages.Core.Regulator;
using Lykke.Job.Messages.Core.Services.SwiftCredentials;
using Lykke.Service.PersonalData.Contract.Models;
using Lykke.Service.SwiftCredentials.Client;

namespace Lykke.Job.Messages.Services.SwiftCredentials
{
    public class SwiftCredentialsService : ISwiftCredentialsService
    {
        private readonly IRegulatorRepository _regulatorRepository;
        private readonly ISwiftCredentialsClient _swiftCredentialsClient;

        public SwiftCredentialsService(
            IRegulatorRepository regulatorRepository,
            ISwiftCredentialsClient swiftCredentialsClient)
        {
            _regulatorRepository = regulatorRepository;
            _swiftCredentialsClient = swiftCredentialsClient;
        }

        public async Task<ISwiftCredentials> GetCredentialsAsync(string assetId, IPersonalData personalData)
        {
            var regulationId = personalData.SpotRegulator ??
                              (await _regulatorRepository.GetByIdOrDefaultAsync(null)).InternalId;
                              
            var creds = await _swiftCredentialsClient.GetForClientAsync(personalData.Id, regulationId, assetId);

            return new Core.Domain.SwiftCredentials.SwiftCredentials
            {
                AssetId = assetId,
                RegulatorId = creds.RegulatorId,
                BIC = creds.Bic,
                PurposeOfPayment = creds.PurposeOfPayment,
                CompanyAddress = creds.CompanyAddress,
                AccountNumber = creds.AccountNumber,
                BankAddress = creds.BankAddress,
                AccountName = creds.AccountName,
                CorrespondentAccount = creds.CorrespondentAccount
            };
        }
    }
}
