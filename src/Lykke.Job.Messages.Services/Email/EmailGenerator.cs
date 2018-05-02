using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Core.Services.SwiftCredentials;
using Lykke.Job.Messages.Core.Services.Templates;
using Lykke.Job.Messages.Core.Util;
using Lykke.Job.Messages.Services.Email.Resources;
using Lykke.Messages.Email.MessageData;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.EmailSender;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Lykke.Service.TemplateFormatter.TemplateModels;

namespace Lykke.Job.Messages.Services.Email
{
    //Exists for back compatibility
    //Do not use it for new emais. Instead, go with events subscription!
    [Obsolete]
    public class EmailGenerator : IEmailGenerator
    {
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly IPersonalDataService _personalDataService;
        private readonly IRemoteTemplateGenerator _templateGenerator;
        private readonly AppSettings.EmailSettings _emailSettings;
        private readonly AppSettings.BlockchainSettings _blockchainSettings;
        private readonly AppSettings.WalletApiSettings _walletApiSettings;
        private readonly ISwiftCredentialsService _swiftCredentialsService;

        public EmailGenerator(
            IAssetsServiceWithCache assetsService, IPersonalDataService personalDataService,
            AppSettings.EmailSettings emailSettings, AppSettings.BlockchainSettings blockchainSettings, AppSettings.WalletApiSettings walletApiSettings,
            IRemoteTemplateGenerator templateGenerator, ISwiftCredentialsService swiftCredentialsService)
        {
            _assetsService = assetsService;
            _personalDataService = personalDataService;
            _templateGenerator = templateGenerator;
            _emailSettings = emailSettings;
            _blockchainSettings = blockchainSettings;
            _walletApiSettings = walletApiSettings;
            _swiftCredentialsService = swiftCredentialsService;
        }

        public Task<EmailMessage> GenerateLykkeCardVisaMsg(string partnerId, LykkeCardVisaData lykkeCardVisaData)
        {
            var templateVm = new LykkeCardVisaTemplate
            {
                Year = lykkeCardVisaData.Year,
                Url = lykkeCardVisaData.Url
            };

            return _templateGenerator.GenerateAsync(partnerId, "LykkeCardMessageTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateWelcomeMsg(string partnerId, RegistrationMessageData kycOkData)
        {
            var templateVm = new BaseTemplate
            {
                Year = kycOkData.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "WelcomeTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateWelcomeFxMsg(string partnerId, KycOkData kycOkData)
        {
            var templateVm = new BaseTemplate
            {
                Year = kycOkData.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "WelcomeFxTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateConfirmEmailMsg(string partnerId, EmailComfirmationData registrationData)
        {
            var templateVm = new EmailVerificationTemplate
            {
                ConfirmationCode = registrationData.ConfirmationCode,
                Year = registrationData.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "EmailConfirmation", templateVm);
        }

        public async Task<EmailMessage> GenerateCashInMsg(string partnerId, CashInData messageData)
        {
            if (messageData.AssetId == null)
            {
                throw new ArgumentException(nameof(messageData.AssetId));
            }

            var asset = await _assetsService.TryGetAssetAsync(messageData.AssetId);

            if (string.IsNullOrEmpty(messageData.AddressExtension))
            {
                var templateVm = new CashInTemplate
                {
                    Multisig = messageData.Address,
                    Year = DateTime.UtcNow.Year.ToString(),
                    AssetName = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId
                };

                return await _templateGenerator.GenerateAsync(partnerId, "CashInTemplate", templateVm);
            }
            else
            {
                var templateVm = new
                {
                    messageData.Address,
                    messageData.AddressName,
                    messageData.AddressExtension,
                    messageData.AddressExtensionName,
                    Year = DateTime.UtcNow.Year.ToString(),
                    AssetName = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId
                };

                return await _templateGenerator.GenerateAsync(partnerId, "CashInWithAddressExtensionTemplate", templateVm);
            }
        }

        public async Task<EmailMessage> GenerateNoRefundDepositDoneMsg(string partnerId, NoRefundDepositDoneData messageData)
        {
            var asset = await FindAssetByBlockchainAssetIdAsync(partnerId, messageData.AssetBcnId);
            var templateVm = new
            {
                AssetName = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.DisplayId,
                Amount = NumberFormatter.FormatNumber(messageData.Amount, asset.Accuracy),
                Year = DateTime.UtcNow.Year
            };

            return await _templateGenerator.GenerateAsync(partnerId, "NoRefundDepositDoneTemplate", templateVm);
        }

        public async Task<EmailMessage> GenerateNoRefundOCashOutMsg(string partnerId, NoRefundOCashOutData messageData)
        {
            var asset = await _assetsService.TryGetAssetAsync(messageData.AssetId);

            if (asset == null)
            {
                return null;
            }
            
            var templateVm = new
            {
                AssetId = asset.DisplayId,
                Amount = NumberFormatter.FormatNumber(messageData.Amount, asset.Accuracy), 
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, messageData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year
            };

            return await _templateGenerator.GenerateAsync(partnerId, "NoRefundOCashOutTemplate", templateVm);
        }

        public async Task<EmailMessage> GenerateBankCashInMsg(string partnerId, BankCashInData messageData)
        {
            var personalData = await _personalDataService.GetAsync(messageData.ClientId);
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
                CompanyAddress = swiftCredentials.CompanyAddress,
                CorrespondentAccount = swiftCredentials.CorrespondentAccount
            };

            return await _templateGenerator.GenerateAsync(partnerId, "BankCashInTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateCashInRefundMsg(string partnerId, CashInRefundData refundData)
        {
            var templateVm = new BtcDepositDoneTempate
            {
                Amount = refundData.Amount,
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, refundData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year,
                ValidDays = refundData.ValidDays > 0 ? refundData.ValidDays : _emailSettings.RefundTimeoutInDays
            };

            return _templateGenerator.GenerateAsync(partnerId, "BtcDepositDoneTemplate", templateVm);
        }

        public async Task<EmailMessage> GenerateSwapRefundMsg(string partnerId, SwapRefundData refundData)
        {
            var templateVm = new SwapDoneTemplate
            {
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, refundData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year,
                ValidDays = refundData.ValidDays > 0 ? refundData.ValidDays : _emailSettings.RefundTimeoutInDays
            };

            var emailMessage = await _templateGenerator.GenerateAsync(partnerId, "SwapDoneTemplate", templateVm);
            AddRefundAttachment(emailMessage, refundData.RefundTransaction);
            return emailMessage;
        }

        public async Task<EmailMessage> GenerateOrdinaryCashOutRefundMsg(string partnerId, OrdinaryCashOutRefundData refundData)
        {
            var templateVm = new OrdinaryCashOutDoneTemplate
            {
                Amount = refundData.Amount,
                AssetId = refundData.AssetId,
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, refundData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year,
                ValidDays = refundData.ValidDays > 0 ? refundData.ValidDays : _emailSettings.RefundTimeoutInDays
            };

            var emailMessage = await _templateGenerator.GenerateAsync(partnerId, "OCashOutDoneTemplate", templateVm);
            AddRefundAttachment(emailMessage, refundData.RefundTransaction);
            return emailMessage;
        }

        public Task<EmailMessage> GenerateTransferCompletedMsg(string partnerId, TransferCompletedData transferCompletedData)
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

            return _templateGenerator.GenerateAsync(partnerId, "TransferCompleteTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateDirectTransferCompletedMsg(string partnerId, DirectTransferCompletedData transferCompletedData)
        {
            var templateVm = new DirectTransferTemplate
            {
                Amount = transferCompletedData.Amount,
                AssetId = transferCompletedData.AssetId,
                ClientName = transferCompletedData.ClientName,
                ExplorerUrl = string.Format(_blockchainSettings.ExplorerUrl, transferCompletedData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "DirectTransferCompleteTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateMyLykkeCashInMsg(string partnerId, MyLykkeCashInData messageData)
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

            return _templateGenerator.GenerateAsync(partnerId, "MyLykkeCashInTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateRemindPasswordMsg(string partnerId, RemindPasswordData messageData)
        {
            var templateVm = new RemindPasswordTemplate
            {
                Hint = messageData.PasswordHint,
                Year = DateTime.UtcNow.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, string.IsNullOrEmpty(messageData.PasswordHint) ? "NoPasswordHintTemplate" : "RemindPasswordTemplate", templateVm);
        }

        public Task<EmailMessage> GeneratPrivateWalletAddressMsg(string partnerId, PrivateWalletAddressData messageData)
        {
            var templateVm = new PrivateWalletAddressTemplate
            {
                Address = messageData.Address,
                Name = messageData.Name,
                Year = DateTime.UtcNow.Year.ToString()
            };

            return _templateGenerator.GenerateAsync(partnerId, "PrivateWalletAddressTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateRestrictedAreaMsg(string partnerId, RestrictedAreaData messageData)
        {
            var templateVm = new RestrictedAreaTemplate
            {
                FirstName = messageData.FirstName,
                LastName = messageData.LastName,
                Year = DateTime.UtcNow.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "RestrictedAreaTemplate", templateVm);
        }

        public Task<EmailMessage> GeneratSolarCashOutMsg(string partnerId, SolarCashOutData messageData)
        {
            var templateVm = new SolarCashOutTemplate
            {
                AddressTo = messageData.AddressTo,
                Amount = messageData.Amount,
                Year = DateTime.UtcNow.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "SolarCashOutTemplate", templateVm);
        }

        public Task<EmailMessage> GeneratSolarAddressMsg(string partnerId, SolarCoinAddressData messageData)
        {
            var templateVm = new SolarCoinAddressTemplate
            {
                Address = messageData.Address,
                Year = DateTime.UtcNow.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "SolarCoinAddressTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateDeclinedDocumentsMsg(string partnerId, DeclinedDocumentsData messageData)
        {
            var documentsAsHtml = new StringBuilder();
            if (null != messageData.Documents)
            {
                foreach (var document in messageData.Documents)
                {
                    string kycDocType = document.Type;
                    switch(document.Type.ToLower())
                    {
                        case "idcard":
                            kycDocType = "Passport or ID";
                            break;
                        case "idcardbackside":
                            kycDocType = "Passport or ID (back side)";
                            break;
                        case "proofofaddress":
                            kycDocType = "Proof of address";
                            break;
                    }

                    documentsAsHtml.AppendLine("<tr style='border-top: 1px solid #8C94A0; border-bottom: 1px solid #8C94A0;'>");
                    documentsAsHtml.AppendLine(
                        $"<td style='padding: 15px 0 15px 0;' width='260'><span style='font-size: 1.1em;color: #8C94A0;'>{kycDocType}</span></td>");
                    documentsAsHtml.AppendLine(
                        $"<td style='padding: 15px 0 15px 0;' width='260'><span style='font-size: 1.1em;color: #3F4D60;'>{HtmlBreaks(document.KycComment)}</span></td>");
                    documentsAsHtml.AppendLine("</tr>");
                }
            }

            var templateVm = new DeclinedDocumentsTemplate
            {
                FullName = messageData.FullName,
                DocumentsAsHtml = documentsAsHtml.ToString(),
                Year = DateTime.UtcNow.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "DeclinedDocumentsTemplate", templateVm);
        }



        public Task<EmailMessage> GenerateCashoutUnlockMsg(string partnerId, CashoutUnlockData messageData)
        {
            var templateVm = new CashoutUnlockTemplate
            {
                Link = $"{_walletApiSettings.Host}/api/cashout-confirm/{messageData.ClientId}?code={messageData.Code}",
                Year = DateTime.UtcNow.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "CashoutUnlockTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateSwiftConfirmedMsg(string partnerId, SwiftConfirmedData data)
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

            return _templateGenerator.GenerateAsync(partnerId, "SwiftConfirmedTemplate", templateVm);
        }

        public async Task<EmailMessage> GenerateSwiftCashOutRequestMsg(string partnerId, SwiftCashOutRequestData messageData)
        {
            var personalData = await _personalDataService.GetAsync(messageData.ClientId);

            var apiHost = _walletApiSettings.Host.Trim().ToLower();
            if (apiHost.Last() != '/')
                apiHost += "/";

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
                ConfirmUrl = $"{apiHost}api/CashOutSwiftRequest/{messageData.CashOutRequestId}?result=true&clientid={messageData.ClientId}",
                DeclineUrl = $"{apiHost}api/CashOutSwiftRequest/{messageData.CashOutRequestId}?result=false&clientid={messageData.ClientId}"
            };

            return await _templateGenerator.GenerateAsync(partnerId, "SwiftCashOutRequest", templateVm);
        }

        private void AddRefundAttachment(EmailMessage emailMessage, string refundData)
        {
            var newAttachments = null != emailMessage.Attachments
                ? new List<EmailAttachment>(emailMessage.Attachments)
                : new List<EmailAttachment>(1);

            newAttachments.Add(new EmailAttachment
            {
                MimeType = MediaTypeNames.Text.Plain,
                FileName = "refund.txt",
                ContentInBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(refundData))
            });

            emailMessage.Attachments = newAttachments.ToArray();
        }

        public Task<EmailMessage> GenerateUserRegisteredMsg(string partnerId, IPersonalData personalData)
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

            return _templateGenerator.GenerateAsync(partnerId, "UserRegisteredTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateRejectedEmailMsg(string partnerId)
        {
            var templateVm = new BaseTemplate
            {
                Year = DateTime.UtcNow.Year.ToString()
            };

            return _templateGenerator.GenerateAsync(partnerId, "RejectedTemplate", templateVm);
        }

        public EmailMessage GenerateFailedTransactionMsg(string partnerId, string transactionId, string[] clientIds)
        {
            return new EmailMessage
            {
                TextBody = GetFailedTransactionBody(transactionId, clientIds),
                Subject = EmailResources.FailedTransaction_Subject
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

        public async Task<EmailMessage> GenerateRequestForDocumentMsg(string partnerId, RequestForDocumentData messageData)
        {
            var templateVm = new RequestForDocumentTemplate
            {
                Text = messageData.Text,
                Comment = messageData.Comment,
                FullName = messageData.FullName,
                Year = DateTime.UtcNow.Year
            };

            return await _templateGenerator.GenerateAsync(partnerId, "RequestForDocument", templateVm);
        }

        public async Task<Asset> FindAssetByBlockchainAssetIdAsync(string partnerId, string blockchainAssetId)
        {
            if (blockchainAssetId == null)
            {
                return await _assetsService.TryGetAssetAsync(LykkeConstants.BitcoinAssetId);
            }

            var assets = await _assetsService.GetAllAssetsAsync();

            return assets.FirstOrDefault(itm => itm.BlockChainAssetId == blockchainAssetId || itm.Id == blockchainAssetId);
        }

        public async Task<EmailMessage> GenerateSwiftCashoutRequestedMsg(string partnerId, SwiftCashoutRequestedData messageData)
        {
            var templateVm = new SwiftCashoutRequestedTemplate
            {
                FullName = messageData.FullName,
                Year = DateTime.UtcNow.Year.ToString()
            };

            return await _templateGenerator.GenerateAsync(partnerId, "SwiftCashoutRequested", templateVm);
        }

        public async Task<EmailMessage> GenerateSwiftCashoutProcessedMsg(string partnerId, SwiftCashoutProcessedData messageData)
        {
            var templateVm = new SwiftCashoutProcessedTemplate
            {
                FullName = messageData.FullName,
                DateOfWithdrawal = messageData.DateOfWithdrawal,
                Year = DateTime.UtcNow.Year.ToString()
            };

            return await _templateGenerator.GenerateAsync(partnerId, "SwiftCashoutProcessed", templateVm);
        }

        public async Task<EmailMessage> GenerateSwiftCashoutDeclinedMsg(string partnerId, SwiftCashoutDeclinedData messageData)
        {
            var templateVm = new SwiftCashoutDeclinedTemplate
            {
                FullName = messageData.FullName,
                Comment = messageData.Comment,
                Text = messageData.Text,
                Year = DateTime.UtcNow.Year.ToString()
            };

            return await _templateGenerator.GenerateAsync(partnerId, "SwiftCashoutDeclined", templateVm);
        }

        public Task<EmailMessage> GenerateRegistrationVerifyEmailMsgAsync(string partnerId, RegistrationEmailVerifyData messageData)
        {
            var templateVm = new RegistrationEmailVerifyData
            {
                Code = messageData.Code,
                Url = messageData.Url,
                Year = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture)
            };

            return _templateGenerator.GenerateAsync(partnerId, "EmailVerificationCode", templateVm);
        }

        public Task<EmailMessage> GenerateWelcomeCypMsg(string partnerId, RegistrationCypMessageData kycOkData)
        {
            var templateVm = new BaseTemplate
            {
                Year = kycOkData.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "WelcomeCypTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateWelcomeFxCypMsg(string partnerId, KycOkCypData kycOkData)
        {
            var templateVm = new BaseTemplate
            {
                Year = kycOkData.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "WelcomeFxCypTemplate", templateVm);
        }

        public Task<EmailMessage> GenerateActionConfirmationMsg(string partnerId, ActionConfirmationData messageData)
        {
            var templateVm = new ActionConfirmationTemplate
            {
                Year = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture),
                ClientName = messageData.ClientName,
                ConfirmationLink = messageData.ConfirmationLink,
                Ip = messageData.Ip
            };

            return _templateGenerator.GenerateAsync(partnerId, "ActionConfirmation", templateVm);
        }

        public Task<EmailMessage> GenerateNoAccountPasswordRecoverydMsg(string partnerId, NoAccountPasswordRecoveryData noAccountData)
        {
            var templateVm = new NoAccountPasswordRecoveryTemplate
            {
                Email = noAccountData.Email,
                Year = DateTime.UtcNow.Year
            };

            return _templateGenerator.GenerateAsync(partnerId, "NoAccountPasswordRecoveryTemplate", templateVm);
        }

        private static string HtmlBreaks(string src)
        {
            return src == null ? "" : src.Replace("\r\n", "<br>");
        }
    }
}
