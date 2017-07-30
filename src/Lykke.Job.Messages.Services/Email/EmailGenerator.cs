using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Core.Domain.Clients;
using Lykke.Job.Messages.Core.Domain.Email;
using Lykke.Job.Messages.Core.Domain.Email.MessagesData;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Core.Services.SwiftCredentials;
using Lykke.Job.Messages.Core.Services.Templates;
using Lykke.Job.Messages.Services.Email.Resources;
using Lykke.Job.Messages.Services.Email.TemplateModels;
using Lykke.Service.Assets.Client.Custom;

namespace Lykke.Job.Messages.Services.Email
{
    public class EmailGenerator : IEmailGenerator
    {
        private readonly ICachedAssetsService _assetsService;
        private readonly IPersonalDataRepository _personalDataRepository;
        private readonly IRemoteTemplateGenerator _templateGenerator;
        private readonly AppSettings.EmailSettings _emailSettings;
        private readonly AppSettings.BlockchainSettings _blockchainSettings;
        private readonly AppSettings.WalletApiSettings _walletApiSettings;
        private readonly ISwiftCredentialsService _swiftCredentialsService;

        public EmailGenerator(
            ICachedAssetsService assetsService, IPersonalDataRepository personalDataRepository, 
            AppSettings.EmailSettings emailSettings, AppSettings.BlockchainSettings blockchainSettings, AppSettings.WalletApiSettings walletApiSettings,
            IRemoteTemplateGenerator templateGenerator, ISwiftCredentialsService swiftCredentialsService)
        {
            _assetsService = assetsService;
            _personalDataRepository = personalDataRepository;
            _templateGenerator = templateGenerator;
            _emailSettings = emailSettings;
            _blockchainSettings = blockchainSettings;
            _walletApiSettings = walletApiSettings;
            _swiftCredentialsService = swiftCredentialsService;
        }

        public async Task<EmailMessage> GenerateWelcomeMsg(RegistrationData kycOkData)
        {
            var templateVm = new BaseTemplate
            {
                Year = kycOkData.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("WelcomeTemplate", templateVm),
                Subject = EmailResources.Welcome_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateWelcomeFxMsg(KycOkData kycOkData)
        {
            var templateVm = new BaseTemplate
            {
                Year = kycOkData.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("WelcomeFxTemplate", templateVm),
                Subject = EmailResources.WelcomeFx_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateConfirmEmailMsg(EmailComfirmationData registrationData)
        {
            var templateVm = new EmailVerificationTemplate
            {
                ConfirmationCode = registrationData.ConfirmationCode,
                Year = registrationData.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("EmailConfirmation", templateVm),
                Subject = EmailResources.EmailConfirmation_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateCashInMsg(CashInData cashInData)
        {
            if (cashInData.AssetId == null)
                throw new ArgumentNullException("AssetId");

            var asset = await _assetsService.TryGetAssetAsync(cashInData.AssetId);
            var templateVm = new CashInTemplate
            {
                Multisig = cashInData.Multisig,
                Year = DateTime.UtcNow.Year.ToString(),
                AssetName = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.Name
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("CashInTemplate", templateVm),
                Subject = string.Format(EmailResources.CashIn_Subject, templateVm.AssetName),
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateNoRefundDepositDoneMsg(NoRefundDepositDoneData messageData)
        {
            var asset = await FindAssetByBlockchainAssetIdAsync(messageData.AssetBcnId);
            var templateVm = new BtcDepositDoneTempate
            {
                AssetName = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.Name,
                Amount = messageData.Amount,
                Year = DateTime.UtcNow.Year
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("NoRefundDepositDoneTemplate", templateVm),
                Subject = string.Format(EmailResources.Deposit_no_refund_done_subject, templateVm.AssetName),
                IsHtml = true
            };

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateNoRefundOCashOutMsg(NoRefundOCashOutData messageData)
        {
            var templateVm = new NoRefundCashOutTemplate
            {
                AssetId = messageData.AssetId,
                Amount = messageData.Amount,
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, messageData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("NoRefundOCashOutTemplate", templateVm),
                Subject = EmailResources.Cash_out_no_refund_subject,
                IsHtml = true
            };

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateBankCashInMsg(BankCashInData messageData)
        {
            var personalData = await _personalDataRepository.GetAsync(messageData.ClientId);
            var asset = await _assetsService.TryGetAssetAsync(messageData.AssetId);
            var swiftCredentials = await _swiftCredentialsService.GetCredentialsAsync(asset.Id, personalData);

            var templateVm = new BankCashInTemplate
            {
                AssetId = messageData.AssetId,
                AssetSymbol = asset.Symbol,
                ClientName = personalData.FullName,
                Amount = messageData.Amount,
                Year = DateTime.UtcNow.Year.ToString(),
                AccountName = swiftCredentials.AccountName,
                AccountNumber = swiftCredentials.AccountNumber,
                Bic = swiftCredentials.BIC,
                PurposeOfPayment = swiftCredentials.PurposeOfPayment,
                BankAddress = swiftCredentials.BankAddress,
                CompanyAddress = swiftCredentials.CompanyAddress
            };

            var msg = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("BankCashInTemplate", templateVm),
                Subject = EmailResources.BankCashIn_Subject,
                IsHtml = true
            };

            //var stream = new MemoryStream();
            //await _srvPdfGenerator.PrintInvoice(stream, messageData.ClientId, messageData.Amount, messageData.AssetId);

            //msg.Attachments = new[]
            //{
            //    new EmailAttachment {ContentType = MediaTypeNames.Application.Pdf,
            //        FileName = "invoice.pdf", Stream = stream}
            //};

            return msg;
        }

        public async Task<EmailMessage> GenerateCashInRefundMsg(CashInRefundData refundData)
        {
            var templateVm = new BtcDepositDoneTempate
            {
                Amount = refundData.Amount,
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, refundData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year,
                ValidDays = refundData.ValidDays > 0 ? refundData.ValidDays : _emailSettings.RefundTimeoutInDays
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("BtcDepositDoneTemplate", templateVm),
                Subject = EmailResources.Deposit_done_Subject,
                IsHtml = true
            };

            AddRefundAttachment(emailMessage, refundData.RefundTransaction);

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateSwapRefundMsg(SwapRefundData refundData)
        {
            var templateVm = new SwapDoneTemplate
            {
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, refundData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year,
                ValidDays = refundData.ValidDays > 0 ? refundData.ValidDays : _emailSettings.RefundTimeoutInDays
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("SwapDoneTemplate", templateVm),
                Subject = EmailResources.Swap_done_Subject,
                IsHtml = true
            };

            AddRefundAttachment(emailMessage, refundData.RefundTransaction);

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateOrdinaryCashOutRefundMsg(OrdinaryCashOutRefundData refundData)
        {
            var templateVm = new OrdinaryCashOutDoneTemplate
            {
                Amount = refundData.Amount,
                AssetId = refundData.AssetId,
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, refundData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year,
                ValidDays = refundData.ValidDays > 0 ? refundData.ValidDays : _emailSettings.RefundTimeoutInDays
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("OCashOutDoneTemplate", templateVm),
                Subject = EmailResources.OrdinaryCashOut_done_Subject,
                IsHtml = true
            };

            AddRefundAttachment(emailMessage, refundData.RefundTransaction);

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateTransferCompletedMsg(TransferCompletedData transferCompletedData)
        {
            const int maxAccuracy = 8;

            var templateVm = new TransferTemplate
            {
                Price = transferCompletedData.Price.GetFixedAsString(maxAccuracy),
                AmountFiat = transferCompletedData.AmountFiat,
                AmountLkk = transferCompletedData.AmountLkk,
                AssetId = transferCompletedData.AssetId,
                ClientName = transferCompletedData.ClientName,
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, transferCompletedData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("TransferCompleteTemplate", templateVm),
                Subject = EmailResources.TransferCompleted_Subject,
                IsHtml = true
            };

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateDirectTransferCompletedMsg(DirectTransferCompletedData transferCompletedData)
        {
            var templateVm = new DirectTransferTemplate
            {
                Amount = transferCompletedData.Amount,
                AssetId = transferCompletedData.AssetId,
                ClientName = transferCompletedData.ClientName,
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, transferCompletedData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("DirectTransferCompleteTemplate", templateVm),
                Subject = EmailResources.TransferCompleted_Subject,
                IsHtml = true
            };

            return emailMessage;
        }
        
        public async Task<EmailMessage> GenerateMyLykkeCashInMsg(MyLykkeCashInData messageData)
        {
            var templateVm = new MyLykkeCashInTemplate
            {
                Amount = messageData.Amount,
                ConversionWalletAddress = messageData.ConversionWalletAddress,
                LkkAmount = messageData.LkkAmount,
                Price = messageData.Price,
                Year = DateTime.UtcNow.Year.ToString(),
                AssetId = messageData.AssetId
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("MyLykkeCashInTemplate", templateVm),
                Subject = string.Format(EmailResources.MyLykkeCashIn_Subject),
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateRemindPasswordMsg(RemindPasswordData messageData)
        {
            var templateVm = new RemindPasswordTemplate(messageData.PasswordHint, DateTime.UtcNow.Year);

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("RemindPasswordTemplate", templateVm),
                Subject = EmailResources.RemindPassword_Subject,
                IsHtml = true
            };

            return emailMessage;
        }

        public async Task<EmailMessage> GeneratPrivateWalletAddressMsg(PrivateWalletAddressData messageData)
        {
            var templateVm = new PrivateWalletAddressTemplate
            {
                Address = messageData.Address,
                Name = messageData.Name,
                Year = DateTime.UtcNow.Year.ToString()
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("PrivateWalletAddressTemplate", templateVm),
                Subject = EmailResources.PrivateWalletAddress_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GeneratSolarCashOutMsg(SolarCashOutData messageData)
        {
            var templateVm = new SolarCashOutTemplate
            {
                AddressTo = messageData.AddressTo,
                Amount = messageData.Amount,
                Year = DateTime.UtcNow.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("SolarCashOutTemplate", templateVm),
                Subject = EmailResources.SolarCashOut_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GeneratSolarAddressMsg(SolarCoinAddressData messageData)
        {
            var templateVm = new SolarCoinAddressTemplate
            {
                Address = messageData.Address,
                Year = DateTime.UtcNow.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("SolarCoinAddressTemplate", templateVm),
                Subject = EmailResources.SolarCoinAddress_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateDeclinedDocumentsMsg(DeclinedDocumentsData messageData)
        {
            var templateVm = new DeclinedDocumentsTemplate
            {
                FullName = messageData.FullName,
                Documents = messageData.Documents,
                Year = DateTime.UtcNow.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("DeclinedDocumentsTemplate", templateVm),
                Subject = EmailResources.DeclinedDocuments_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateCashoutUnlockMsg(CashoutUnlockData messageData)
        {
            var templateVm = new CashoutUnlockTemplate
            {
                Link = $"{_walletApiSettings.Host}/api/cashout-confirm/{messageData.ClientId}?code={messageData.Code}",
                Year = DateTime.UtcNow.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("CashoutUnlockTemplate", templateVm),
                Subject = EmailResources.WithdrawalRequest_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateSwiftConfirmedMsg(SwiftConfirmedData data)
        {
            var templateVm = new SwiftConfirmedTemplate
            {
                Amount = data.Amount,
                Email = data.Email,
                AccName = data.AccName,
                AccNumber = data.AccNumber,
                AssetId = data.AssetId,
                Bic = data.Bic,
                AccHolderAddress = data.AccHolderAddress,
                BankName = data.BankName,
                ExplorerUrl = !string.IsNullOrEmpty(data.BlockchainHash)
                    ? string.Format(_blockchainSettings.ExplorerUrl, data.BlockchainHash)
                    : string.Empty
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("SwiftConfirmedTemplate", templateVm),
                Subject = EmailResources.SwiftConfirmed_subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateSwiftCashOutRequestMsg(SwiftCashOutRequestData messageData)
        {
            var personalData = await _personalDataRepository.GetAsync(messageData.ClientId);

            var templateVm = new SwiftCashOutRequestTemplate
            {
                Amount = messageData.Amount,
                Year = DateTime.UtcNow.Year,
                AssetId = messageData.AssetId,
                AccName = messageData.AccName,
                AccNum = messageData.AccNum,
                Bic = messageData.Bic,
                BankName = messageData.BankName,
                AccHolderAddress = messageData.AccHolderAddress,
                FullName = personalData.FullName,
                ConfirmUrl = $"{_walletApiSettings.Host}/api/CashOutSwiftRequest/{messageData.CashOutRequestId}?result=true&clientid={messageData.ClientId}",
                DeclineUrl = $"{_walletApiSettings.Host}/api/CashOutSwiftRequest/{messageData.CashOutRequestId}?result=false&clientid={messageData.ClientId}"
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("SwiftCashOutRequest", templateVm),
                Subject = EmailResources.SwiftCashOutRequest_Subject,
                IsHtml = true
            };
        }

        private void AddRefundAttachment(EmailMessage emailMessage, string refundData)
        {
            emailMessage.Attachments = new[]
            {
                new EmailAttachment {ContentType = MediaTypeNames.Text.Plain,
                    FileName = "refund.txt", Stream = new MemoryStream(Encoding.UTF8.GetBytes(refundData))
                }
            };
        }

        public async Task<EmailMessage> GenerateUserRegisteredMsg(IPersonalData personalData)
        {
            var templateVm = new UserRegisteredTemplate
            {
                ContactPhone = personalData.ContactPhone,
                Country = personalData.Country,
                DateTime = personalData.Regitered,
                Email = personalData.Email,
                FullName = personalData.FullName,
                UserId = personalData.Id
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("UserRegisteredTemplate", templateVm),
                Subject = EmailResources.UserRegistered_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateRejectedEmailMsg()
        {
            var templateVm = new BaseTemplate
            {
                Year = DateTime.UtcNow.Year.ToString()
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("RejectedTemplate", templateVm),
                Subject = EmailResources.Rejected_Subject,
                IsHtml = true
            };
        }

        public EmailMessage GenerateFailedTransactionMsg(string transactionId, string[] clientIds)
        {
            return new EmailMessage
            {
                Body = GetFailedTransactionBody(transactionId, clientIds),
                Subject = EmailResources.FailedTransaction_Subject,
                IsHtml = false
            };
        }

        private string GetFailedTransactionBody(string transactionId, string[] clientIds)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Transaction failed: {transactionId}.");
            sb.AppendLine("Affected clients: ");
            foreach (var id in clientIds.Distinct())
            {
                sb.AppendLine(id);
            }

            return sb.ToString();
        }

        public async Task<EmailMessage> GenerateRequestForDocumentMsg(RequestForDocumentData messageData)
        {
            var personalData = await _personalDataRepository.GetAsync(messageData.ClientId);

            var templateVm = new RequestForDocumentTemplate
            {
                Text = messageData.Text,
                Comment = messageData.Comment,
                FullName = personalData.FullName,
                ClientId = messageData.ClientId,
                Amount = messageData.Amount,
                AssetId = messageData.AssetId
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("RequestForDocument", templateVm),
                Subject = EmailResources.RequestForDocument_Subject,
                IsHtml = true
            };
        }

        public async Task<IAsset> FindAssetByBlockchainAssetIdAsync(string blockchainAssetId)
        {
            if (blockchainAssetId == null)
            {
                return await _assetsService.TryGetAssetAsync(LykkeConstants.BitcoinAssetId);
            }

            var assets = await _assetsService.GetAllAssetsAsync();

            return assets.FirstOrDefault(itm => itm.BlockChainAssetId == blockchainAssetId || itm.Id == blockchainAssetId);
        }

        public async Task<EmailMessage> GenerateSwiftCashoutProcessedMsg(SwiftCashoutProcessedData messageData)
        {
            var templateVm = new SwiftCashoutProcessedTemplate
            {
                FullName = messageData.FullName,
                Amount = messageData.Amount,
                Year = DateTime.UtcNow.Year,
                AssetId = messageData.AssetId,
                AccName = messageData.AccName,
                AccNum = messageData.AccNum,
                Bic = messageData.Bic,
                BankName = messageData.BankName,
                AccHolderAddress = messageData.AccHolderAddress                
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync(messageData.MessageId(), templateVm),
                Subject = EmailResources.GenerateSwiftCashoutProcessed_Subject,
                IsHtml = true
            };
        }
    }
}