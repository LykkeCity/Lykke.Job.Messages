using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Clients;
using Lykke.Job.Messages.Core.Domain.SwiftCredentials;
using Lykke.Job.Messages.Core.Regulator;
using Lykke.Job.Messages.Core.Services.SwiftCredentials;
using Lykke.Service.Assets.Client.Custom;
using Lykke.Job.Messages.Core.Domain.DepositRefId;
using System;
using System.Globalization;
using Lykke.Job.Messages.Services.Deposits;

namespace Lykke.Job.Messages.Services.SwiftCredentials
{
    public class SwiftCredentialsService : ISwiftCredentialsService
    {
        private readonly IRegulatorRepository _regulatorRepository;
        private readonly ISwiftCredentialsRepository _swiftCredentialsRepository;
        private readonly ICachedAssetsService _assetsService;
        private readonly IDepositRefIdInUseRepository _depositRefIdInUseRepository;
        private readonly IDepositRefIdRepository _depositRefIdRepository;

        public SwiftCredentialsService(
            IRegulatorRepository regulatorRepository,
            ISwiftCredentialsRepository swiftCredentialsRepository,
            IDepositRefIdInUseRepository depositRefIdInUseRepository,
            IDepositRefIdRepository depositRefIdRepository,
            ICachedAssetsService assetsService)
        {
            _regulatorRepository = regulatorRepository;
            _swiftCredentialsRepository = swiftCredentialsRepository;
            _depositRefIdInUseRepository = depositRefIdInUseRepository;
            _depositRefIdRepository = depositRefIdRepository;
            _assetsService = assetsService;
        }

        public async Task<ISwiftCredentials> GetCredentialsAsync(string assetId, double amount, IPersonalData personalData)
        {
            var regulatorId = personalData.SpotRegulator ??
                              (await _regulatorRepository.GetByIdOrDefaultAsync(null)).InternalId;

            //if no credentials, try to get default for regulator
            var credentials = await _swiftCredentialsRepository.GetCredentialsAsync(regulatorId, assetId) ??
                              await _swiftCredentialsRepository.GetCredentialsAsync(regulatorId);

            return await BuildCredentials(assetId, amount, credentials, personalData);
        }

        private async Task<ISwiftCredentials> BuildCredentials(string assetId, double amount, ISwiftCredentials sourceCredentials, IPersonalData personalData)
        {
            if (sourceCredentials == null)
                return null;

            var asset = await _assetsService.TryGetAssetAsync(assetId);
            var assetTitle = asset?.DisplayId ?? assetId;

            string clientIdentity = null;
            string purposeOfPayment = null;

            clientIdentity = personalData != null ? await DepositRefIdGenService.GenerateReferenceId(assetId, amount, personalData.Email, personalData.Id, _depositRefIdInUseRepository) : "{1}";
            //clientIdentity = personalData != null ? personalData.Email.Replace("@", ".") : "{1}";
            purposeOfPayment = string.Format(sourceCredentials.PurposeOfPayment, assetTitle, clientIdentity);

            if (!purposeOfPayment.Contains(assetId) && !purposeOfPayment.Contains(assetTitle))
                purposeOfPayment += assetTitle;

            if (!purposeOfPayment.Contains(clientIdentity))
                purposeOfPayment += clientIdentity;

            return new Core.Domain.SwiftCredentials.SwiftCredentials
            {
                AssetId = assetId,
                RegulatorId = sourceCredentials.RegulatorId,
                BIC = sourceCredentials.BIC,
                PurposeOfPayment = purposeOfPayment,
                CompanyAddress = sourceCredentials.CompanyAddress,
                AccountNumber = sourceCredentials.AccountNumber,
                BankAddress = sourceCredentials.BankAddress,
                AccountName = sourceCredentials.AccountName
            };
        }
    }
}