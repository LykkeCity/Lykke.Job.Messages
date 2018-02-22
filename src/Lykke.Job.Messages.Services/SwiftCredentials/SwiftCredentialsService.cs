using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.SwiftCredentials;
using Lykke.Job.Messages.Core.Services.SwiftCredentials;
using Lykke.Service.Assets.Client;
using Lykke.Job.Messages.Core.Domain.DepositRefId;
using System;
using System.Globalization;
using System.Linq;
using Common;
using Common.Log;
using Lykke.Service.PersonalData.Contract.Models;
using Lykke.Service.SwiftCredentials.Client;
using Lykke.Service.SwiftCredentials.Client.Models;

namespace Lykke.Job.Messages.Services.SwiftCredentials
{
    public class SwiftCredentialsService : ISwiftCredentialsService
    {
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;
        private readonly IAssetsService _assetsService;
        private readonly ILog _log;
        private readonly ISwiftCredentialsClient _swiftCredentialsClient;
        private readonly IDepositRefIdInUseRepository _depositRefIdInUseRepository;
        private readonly IDepositRefIdRepository _depositRefIdRepository;

        public SwiftCredentialsService(
            ISwiftCredentialsClient swiftCredentialsClient,
            IDepositRefIdInUseRepository depositRefIdInUseRepository,
            IDepositRefIdRepository depositRefIdRepository,
            IAssetsServiceWithCache assetsServiceWithCache,
            IAssetsService assetsService,
            ILog log)
        {
            _swiftCredentialsClient = swiftCredentialsClient;
            _depositRefIdInUseRepository = depositRefIdInUseRepository;
            _depositRefIdRepository = depositRefIdRepository;
            _assetsServiceWithCache = assetsServiceWithCache;
            _assetsService = assetsService;
            _log = log;
        }

        public async Task<ISwiftCredentials> GetCredentialsAsync(string assetId, IPersonalData personalData)
        {
            var assetConditions = await _assetsService.ClientGetAssetConditionsAsync(personalData.Id);

            var regulationId = assetConditions.FirstOrDefault(o => o.Asset == assetId)?.Regulation;

            if (string.IsNullOrEmpty(regulationId))
            {
                await _log.WriteWarningAsync(nameof(SwiftCredentialsService), nameof(GetCredentialsAsync),
                    new {AssetId = assetId, ClientId = personalData.Id}.ToJson(),
                    "Regulation is undefined");

                return null;
            }

            SwiftCredentialsModel swiftCredentials;
            
            try
            {
                swiftCredentials = await _swiftCredentialsClient.GetAsync(regulationId, assetId);
            }
            catch (Service.SwiftCredentials.Client.Exceptions.ErrorResponseException exception)
            {
                await _log.WriteErrorAsync(nameof(SwiftCredentialsService), nameof(GetCredentialsAsync), exception);
                return null;
            }

            return await BuildCredentialsAsync(assetId, swiftCredentials, personalData);
        }

        private async Task<ISwiftCredentials> BuildCredentialsAsync(
            string assetId,
            SwiftCredentialsModel swiftCredentials,
            IPersonalData personalData)
        {
            if (swiftCredentials == null)
                return null;

            var asset = await _assetsServiceWithCache.TryGetAssetAsync(assetId);
            var assetTitle = asset?.DisplayId ?? assetId;

            string clientIdentity;
            string purposeOfPayment;

            DateTime d1 = DateTime.Now;
            string date = d1.ToString("ddMMMyyyy", CultureInfo.InvariantCulture);
            IDepositRefIdInUse refId = await _depositRefIdInUseRepository.GetRefIdAsync(personalData.Id, date, assetId);
            if (refId == null)
            {
                // maybe a day has just been changed from yestrday to today
                // so it is required to check yesterday's ref ids
                date = d1.ToString("ddMMMyyyy", CultureInfo.InvariantCulture);
                refId = await _depositRefIdInUseRepository.GetRefIdAsync(personalData.Id, date, assetId);
            }
            if (refId != null) // ref id has been found
            {
                string email = personalData.Email.Replace("@", "..");
                clientIdentity = $"{email}_{date}_{refId.Code}";
                _depositRefIdRepository.AddCodeAsync(clientIdentity, refId.ClientId, date, refId.Code);
                purposeOfPayment = string.Format(swiftCredentials.PurposeOfPayment, assetTitle, clientIdentity);
            }
            else
            {
                clientIdentity = personalData != null ? personalData.Email.Replace("@", ".") : "{1}";
                purposeOfPayment = string.Format(swiftCredentials.PurposeOfPayment, assetTitle, clientIdentity);

                if (!purposeOfPayment.Contains(assetId) && !purposeOfPayment.Contains(assetTitle))
                    purposeOfPayment += assetTitle;

                if (!purposeOfPayment.Contains(clientIdentity))
                    purposeOfPayment += clientIdentity;
            }

            return new Core.Domain.SwiftCredentials.SwiftCredentials
            {
                AssetId = assetId,
                RegulatorId = swiftCredentials.RegulationId,
                BIC = swiftCredentials.Bic,
                PurposeOfPayment = purposeOfPayment,
                CompanyAddress = swiftCredentials.CompanyAddress,
                AccountNumber = swiftCredentials.AccountNumber,
                BankAddress = swiftCredentials.BankAddress,
                AccountName = swiftCredentials.AccountName,
                CorrespondentAccount = swiftCredentials.CorrespondentAccount
            };
        }
    }
}