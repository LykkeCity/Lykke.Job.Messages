using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Lykke.Job.Messages.Contract.Emails;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Messages.Email.MessageData;
using Lykke.Service.EmailSender;
using Lykke.Service.PersonalData.Contract;

namespace Lykke.Job.Messages.QueueConsumers
{
    public class EmailQueueConsumer
    {
        private readonly IEnumerable<IQueueReader> _queueReadersList;
        private readonly ISmtpEmailSender _smtpEmailSender;
        private readonly IEmailGenerator _emailGenerator;
        private readonly IPersonalDataService _personalDataService;
        private readonly ILog _log;

        public EmailQueueConsumer(IEnumerable<IQueueReader> queueReadersList, ISmtpEmailSender smtpEmailSender,
            IEmailGenerator emailGenerator, IPersonalDataService personalDataService, ILog log)
        {
            _queueReadersList = queueReadersList;
            _smtpEmailSender = smtpEmailSender;
            _emailGenerator = emailGenerator;
            _personalDataService = personalDataService;
            _log = log.CreateComponentScope(nameof(EmailQueueConsumer));

            InitQueues();
        }

        private void InitQueues()
        {
            foreach (var queueReader in _queueReadersList)
            {
                queueReader.RegisterPreHandler(async data =>
                {
                    if (data == null)
                    {
                        _log.WriteInfo(nameof(InitQueues), null, "Queue had unknown message");
                        return false;
                    }
                    return true;
                });

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RegistrationMessageData>>>(
                    RegistrationMessageData.QueueName, itm => HandleRegisteredEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<KycOkData>>>(
                    KycOkData.QueueName, itm => HandleKycOkEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<EmailComfirmationData>>>(
                    EmailComfirmationData.QueueName, itm => HandleConfirmEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<CashInData>>>(
                    CashInData.QueueName, itm => HandleCashInEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<NoRefundDepositDoneData>>>(
                    NoRefundDepositDoneData.QueueName, itm => HandleNoRefundDepositDoneEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<NoRefundOCashOutData>>>(
                    NoRefundOCashOutData.QueueName, itm => HandleNoRefundOCashOutEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<BankCashInData>>>(
                    BankCashInData.QueueName, itm => HandleBankCashInEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SwiftCashOutRequestData>>>(
                    SwiftCashOutRequestData.QueueName, itm => HandleSwiftCashOutRequestAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RejectedData>>>(
                    RejectedData.QueueName, itm => HandleRejectedEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<CashInRefundData>>>(
                    CashInRefundData.QueueName, itm => HandleCashInRefundEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SwapRefundData>>>(
                    SwapRefundData.QueueName, itm => HandleSwapRefundEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<OrdinaryCashOutRefundData>>>(
                    OrdinaryCashOutRefundData.QueueName, itm => HandleOCashOutRefundEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<TransferCompletedData>>>(
                    TransferCompletedData.QueueName, itm => HandleTransferCompletedEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<DirectTransferCompletedData>>>(
                    DirectTransferCompletedData.QueueName, itm => HandleDirectTransferCompletedEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<PlainTextData>>>(
                    PlainTextData.QueueName, itm => HandlePlainTextEmail(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<MyLykkeCashInData>>>(
                    MyLykkeCashInData.QueueName, itm => HandleMyLykkeCashInEmail(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RemindPasswordData>>>(
                    RemindPasswordData.QueueName, itm => HandleRemindPasswordEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendBroadcastData<UserRegisteredData>>>(
                    UserRegisteredData.QueueName, itm => HandleUserRegisteredBroadcastAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendBroadcastData<SwiftConfirmedData>>>(
                    SwiftConfirmedData.QueueName, itm => HandleSwiftConfirmedBroadcastAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendBroadcastData<PlainTextBroadCastData>>>(
                    PlainTextBroadCastData.QueueName, itm => HandlePlainTextBroadcastAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendBroadcastData<FailedTransactionData>>>(
                    FailedTransactionData.QueueName, itm => HandleFailedTransactionBroadcastAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<PrivateWalletAddressData>>>(
                    PrivateWalletAddressData.QueueName, itm => HandlePrivateWalletAddressEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RestrictedAreaData>>>(
                    RestrictedAreaData.QueueName, itm => HandleRestrictedAreaEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SolarCashOutData>>>(
                    SolarCashOutData.QueueName, itm => HandleSolarCashOutCompletedEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SolarCoinAddressData>>>(
                    SolarCoinAddressData.QueueName, itm => HandleSolarCoinAddressEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<DeclinedDocumentsData>>>(
                    DeclinedDocumentsData.QueueName, itm => HandleDeclinedDocumentsEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<CashoutUnlockData>>>(
                    CashoutUnlockData.QueueName, itm => HandleCashoutUnlockEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RequestForDocumentData>>>(
                    RequestForDocumentData.QueueName, itm => HandleRequestForDocumentEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SwiftCashoutRequestedData>>>(
                    SwiftCashoutRequestedData.QueueName, itm => HandleSwiftCashoutRequestedAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SwiftCashoutProcessedData>>>(
                    SwiftCashoutProcessedData.QueueName, itm => HandleSwiftCashoutProcessedEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SwiftCashoutDeclinedData>>>(
                    SwiftCashoutDeclinedData.QueueName, itm => HandleSwiftCashoutDeclinedEmailAsync(itm.Data));
                
                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RegistrationEmailVerifyData>>>(
                    RegistrationEmailVerifyData.QueueName, itm => HandleRegistrationVerifyEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<LykkeCardVisaData>>>(
                    LykkeCardVisaData.QueueName, itm => HandleLykkeVisaCardEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RegistrationCypMessageData>>>(
                    RegistrationCypMessageData.QueueName, itm => HandleRegisteredCypEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<KycOkCypData>>>(
                    KycOkCypData.QueueName, itm => HandleKycOkCypEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<EmailComfirmationCypData>>>(
                    EmailComfirmationCypData.QueueName, itm => HandleConfirmCypEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<DirectTransferCompletedCypData>>>(
                    DirectTransferCompletedCypData.QueueName, itm => HandleDirectTransferCompletedCypEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<NoAccountPasswordRecoveryData>>>(
                    NoAccountPasswordRecoveryData.QueueName, itm => HandleNoAccountPasswordRecoveryEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SwiftCashoutProcessedCypData>>>(
                   SwiftCashoutProcessedCypData.QueueName, itm => HandleSwiftCashoutProcessedCypEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SwiftCashoutDeclinedCypData>>>(
                   SwiftCashoutDeclinedCypData.QueueName, itm => HandleSwiftCashoutDeclinedCypEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RejectedCypData>>>(
                  RejectedCypData.QueueName, itm => HandleRejectedCypEmailAsync(itm.Data));
            }
        }

        private async Task HandleLykkeVisaCardEmailAsync(SendEmailData<LykkeCardVisaData> result)
         {
            _log.WriteInfo(nameof(HandleLykkeVisaCardEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var lykkeVisaCardData = new LykkeCardVisaData
             {
                 Url = result.MessageData.Url,
                Year = result.MessageData.Year
             };
 
             var msg = await _emailGenerator.GenerateLykkeCardVisaMsg(result.PartnerId, lykkeVisaCardData);
             await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
         }

        private async Task HandleRejectedEmailAsync(SendEmailData<RejectedData> result)
        {
            _log.WriteInfo(nameof(HandleRejectedEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRejectedEmailMsg(result.PartnerId);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRegisteredEmailAsync(SendEmailData<RegistrationMessageData> result)
        {
            _log.WriteInfo(nameof(HandleRegisteredEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var registerData = new RegistrationMessageData
            {
                ClientId = result.MessageData.ClientId,
                Year = result.MessageData.Year
            };

            var msg = await _emailGenerator.GenerateWelcomeMsg(result.PartnerId, registerData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleKycOkEmailAsync(SendEmailData<KycOkData> result)
        {
            _log.WriteInfo(nameof(HandleKycOkEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateWelcomeFxMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleConfirmEmailAsync(SendEmailData<EmailComfirmationData> result)
        {
            _log.WriteInfo(nameof(HandleConfirmEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateConfirmEmailMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleCashInEmailAsync(SendEmailData<CashInData> result)
        {
            _log.WriteInfo(nameof(HandleCashInEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashInMsg(result.PartnerId, result.MessageData);

            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashOutRequestAsync(SendEmailData<SwiftCashOutRequestData> result)
        {
            _log.WriteInfo(nameof(HandleSwiftCashOutRequestAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashOutRequestMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleNoRefundDepositDoneEmailAsync(SendEmailData<NoRefundDepositDoneData> result)
        {
            _log.WriteInfo(nameof(HandleNoRefundDepositDoneEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoRefundDepositDoneMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleNoRefundOCashOutEmailAsync(SendEmailData<NoRefundOCashOutData> result)
        {
            _log.WriteInfo(nameof(HandleNoRefundOCashOutEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoRefundOCashOutMsg(result.PartnerId, result.MessageData);

            if (msg == null)
            {
                _log.WriteWarning(nameof(HandleNoRefundOCashOutEmailAsync), null, "Email was not generated");

                return;
            }

            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleBankCashInEmailAsync(SendEmailData<BankCashInData> result)
        {
            _log.WriteInfo(nameof(HandleBankCashInEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateBankCashInMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePlainTextBroadcastAsync(SendBroadcastData<PlainTextBroadCastData> result)
        {
            _log.WriteInfo(nameof(HandlePlainTextBroadcastAsync), null, $"Broadcast group: {result.BroadcastGroup}");
            var msg = new EmailMessage
            {
                TextBody = result.MessageData.Text,
                Subject = $"[{result.BroadcastGroup}] {result.MessageData.Subject}"
            };
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleUserRegisteredBroadcastAsync(SendBroadcastData<UserRegisteredData> result)
        {
            _log.WriteInfo(nameof(HandleUserRegisteredBroadcastAsync), null, $"Broadcast group: {result.BroadcastGroup}");
            var personalData = await _personalDataService.GetAsync(result.MessageData.ClientId);
            var msg = await _emailGenerator.GenerateUserRegisteredMsg(result.PartnerId, personalData);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleSwiftConfirmedBroadcastAsync(SendBroadcastData<SwiftConfirmedData> result)
        {
            _log.WriteInfo(nameof(HandleSwiftConfirmedBroadcastAsync), null, $"Broadcast group: {result.BroadcastGroup}");            
            var msg = await _emailGenerator.GenerateSwiftConfirmedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleCashInRefundEmailAsync(SendEmailData<CashInRefundData> result)
        {
            _log.WriteInfo(nameof(HandleCashInRefundEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashInRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwapRefundEmailAsync(SendEmailData<SwapRefundData> result)
        {
            _log.WriteInfo(nameof(HandleSwapRefundEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwapRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleOCashOutRefundEmailAsync(SendEmailData<OrdinaryCashOutRefundData> result)
        {
            _log.WriteInfo(nameof(HandleOCashOutRefundEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateOrdinaryCashOutRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleFailedTransactionBroadcastAsync(SendBroadcastData<FailedTransactionData> result)
        {
            _log.WriteInfo(nameof(HandleFailedTransactionBroadcastAsync), null, $"Broadcast group: {result.BroadcastGroup}");
            var msg = _emailGenerator.GenerateFailedTransactionMsg(result.PartnerId, result.MessageData.TransactionId, result.MessageData.AffectedClientIds);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleTransferCompletedEmailAsync(SendEmailData<TransferCompletedData> result)
        {
            _log.WriteInfo(nameof(HandleTransferCompletedEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateTransferCompletedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleDirectTransferCompletedEmailAsync(SendEmailData<DirectTransferCompletedData> result)
        {
            _log.WriteInfo(nameof(HandleDirectTransferCompletedEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateDirectTransferCompletedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePlainTextEmail(SendEmailData<PlainTextData> result)
        {
            _log.WriteInfo(nameof(HandlePlainTextEmail), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = new EmailMessage
            {
                TextBody = result.MessageData.Text,
                Subject = result.MessageData.Subject
            };
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg, result.MessageData.Sender);
        }

        private async Task HandleMyLykkeCashInEmail(SendEmailData<MyLykkeCashInData> result)
        {
            _log.WriteInfo(nameof(HandleMyLykkeCashInEmail), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateMyLykkeCashInMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRemindPasswordEmailAsync(SendEmailData<RemindPasswordData> result)
        {
            _log.WriteInfo(nameof(HandleRemindPasswordEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRemindPasswordMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePrivateWalletAddressEmailAsync(SendEmailData<PrivateWalletAddressData> result)
        {
            _log.WriteInfo(nameof(HandlePrivateWalletAddressEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratPrivateWalletAddressMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRestrictedAreaEmailAsync(SendEmailData<RestrictedAreaData> result)
        {
            _log.WriteInfo(nameof(HandleRestrictedAreaEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRestrictedAreaMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSolarCashOutCompletedEmailAsync(SendEmailData<SolarCashOutData> result)
        {
            _log.WriteInfo(nameof(HandleSolarCashOutCompletedEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratSolarCashOutMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSolarCoinAddressEmailAsync(SendEmailData<SolarCoinAddressData> result)
        {
            _log.WriteInfo(nameof(HandleSolarCoinAddressEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratSolarAddressMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleDeclinedDocumentsEmailAsync(SendEmailData<DeclinedDocumentsData> result)
        {
            _log.WriteInfo(nameof(HandleDeclinedDocumentsEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateDeclinedDocumentsMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleCashoutUnlockEmailAsync(SendEmailData<CashoutUnlockData> result)
        {
            _log.WriteInfo(nameof(HandleCashoutUnlockEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashoutUnlockMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRequestForDocumentEmailAsync(SendEmailData<RequestForDocumentData> result)
        {
            _log.WriteInfo(nameof(HandleRequestForDocumentEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRequestForDocumentMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutRequestedAsync(SendEmailData<SwiftCashoutRequestedData> result)
        {
            _log.WriteInfo(nameof(HandleSwiftCashoutRequestedAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutRequestedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutProcessedEmailAsync(SendEmailData<SwiftCashoutProcessedData> result)
        {
            _log.WriteInfo(nameof(HandleSwiftCashoutProcessedEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutProcessedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutDeclinedEmailAsync(SendEmailData<SwiftCashoutDeclinedData> result)
        {
            _log.WriteInfo(nameof(HandleSwiftCashoutDeclinedEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutDeclinedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }
        
        private async Task HandleRegistrationVerifyEmailAsync(SendEmailData<RegistrationEmailVerifyData> result)
        {
            _log.WriteInfo(nameof(HandleRegistrationVerifyEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRegistrationVerifyEmailMsgAsync(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRegisteredCypEmailAsync(SendEmailData<RegistrationCypMessageData> result)
        {
            _log.WriteInfo(nameof(HandleRegisteredCypEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateWelcomeCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        private async Task HandleKycOkCypEmailAsync(SendEmailData<KycOkCypData> result)
        {
            _log.WriteInfo(nameof(HandleKycOkCypEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateWelcomeFxCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        private async Task HandleConfirmCypEmailAsync(SendEmailData<EmailComfirmationCypData> result)
        {
            _log.WriteInfo(nameof(HandleConfirmCypEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateConfirmEmailCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        private async Task HandleDirectTransferCompletedCypEmailAsync(SendEmailData<DirectTransferCompletedCypData> result)
        {
            _log.WriteInfo(nameof(HandleDirectTransferCompletedCypEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateDirectTransferCompletedCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        private async Task HandleNoAccountPasswordRecoveryEmailAsync(SendEmailData<NoAccountPasswordRecoveryData> result)
        {
            _log.WriteInfo(nameof(HandleNoAccountPasswordRecoveryEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoAccountPasswordRecoveryMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutProcessedCypEmailAsync(SendEmailData<SwiftCashoutProcessedCypData> result)
        {
            _log.WriteInfo(nameof(HandleSwiftCashoutProcessedCypEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutProcessedCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }
        private async Task HandleSwiftCashoutDeclinedCypEmailAsync(SendEmailData<SwiftCashoutDeclinedCypData> result)
        {
            _log.WriteInfo(nameof(HandleSwiftCashoutDeclinedCypEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutDeclinedCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }
        private async Task HandleRejectedCypEmailAsync(SendEmailData<RejectedCypData> result)
        {
            _log.WriteInfo(nameof(HandleRejectedCypEmailAsync), null, $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRejectedEmailCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }
        

        public void Start()
        {
            foreach (var queueReader in _queueReadersList)
            {
                queueReader.Start();
                _log.WriteInfo(nameof(Start), null, $"Started:{queueReader.GetComponentName()}");
            }
        }
    }
}
