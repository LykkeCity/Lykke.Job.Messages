using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Contract.Emails;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Messages.Email.MessageData;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.EmailSender;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.PushNotifications.Contract.Commands;
using Lykke.Service.PushNotifications.Contract.Enums;
using Lykke.Service.TemplateFormatter.Client;

namespace Lykke.Job.Messages.QueueConsumers
{
    public class EmailQueueConsumer : IStartable
    {
        private readonly IEnumerable<IQueueReader> _queueReadersList;
        private readonly ISmtpEmailSender _smtpEmailSender;
        private readonly IEmailGenerator _emailGenerator;
        private readonly IPersonalDataService _personalDataService;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly ITemplateFormatter _templateFormatter;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ILog _log;

        public EmailQueueConsumer(IEnumerable<IQueueReader> queueReadersList, ISmtpEmailSender smtpEmailSender,
            IEmailGenerator emailGenerator, IPersonalDataService personalDataService,
            IClientAccountClient clientAccountClient,
            ITemplateFormatter templateFormatter,
            ICqrsEngine cqrsEngine,
            ILogFactory logFactory)
        {
            _queueReadersList = queueReadersList;
            _smtpEmailSender = smtpEmailSender;
            _emailGenerator = emailGenerator;
            _personalDataService = personalDataService;
            _clientAccountClient = clientAccountClient;
            _templateFormatter = templateFormatter;
            _cqrsEngine = cqrsEngine;
            _log = logFactory.CreateLog(this);

            InitQueues();
        }

        private void InitQueues()
        {
            foreach (var queueReader in _queueReadersList)
            {
                queueReader.RegisterPreHandler(data =>
                {
                    if (data == null)
                    {
                        _log.Warning(nameof(InitQueues), "Queue had unknown message");
                        return Task.FromResult(false);
                    }
                    return Task.FromResult(true);
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

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RemindPasswordCypData>>>(
                    RemindPasswordCypData.QueueName, itm => HandleRemindPasswordCypEmailAsync(itm.Data));

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

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<NoAccountPasswordRecoveryCypData>>>(
                    NoAccountPasswordRecoveryCypData.QueueName, itm => HandleNoAccountPasswordRecoveryCypEmailAsync(itm.Data));

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
            _log.Info(nameof(HandleLykkeVisaCardEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
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
            _log.Info(nameof(HandleRejectedEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRejectedEmailMsg(result.PartnerId);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRegisteredEmailAsync(SendEmailData<RegistrationMessageData> result)
        {
            _log.Info(nameof(HandleRegisteredEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var registerData = new RegistrationMessageData
            {
                ClientId = result.MessageData.ClientId,
                Year = result.MessageData.Year
            };

            var msg = await _emailGenerator.GenerateWelcomeMsg(result.PartnerId, registerData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);

            // TODO: temp solution - to notify user about private key backup, can be removed once backup view will be returned on the mobile applications
            var remindMsg = await _emailGenerator.GenerateRemindBackupMsg(result.PartnerId, registerData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, remindMsg);

            var clientAccountTask = _clientAccountClient.GetByIdAsync(result.MessageData.ClientId);
            var pushSettingsTask = _clientAccountClient.GetPushNotificationAsync(result.MessageData.ClientId);

            await Task.WhenAll(clientAccountTask, pushSettingsTask);

            var clientAccount = clientAccountTask.Result;

            if (!pushSettingsTask.Result.Enabled || string.IsNullOrEmpty(clientAccount?.NotificationsId))
                return;

            var template = await _templateFormatter.FormatAsync(
                "PushRemindBackupTemplate",
                clientAccount.PartnerId,
                "EN");

            if (template != null)
            {
                _cqrsEngine.SendCommand(new TextNotificationCommand
                {
                    NotificationIds = new[] {clientAccount.NotificationsId},
                    Message = template.Subject,
                    Type = NotificationType.Info.ToString()
                }, JobMessagesBoundedContext.Name, PushNotificationsBoundedContext.Name);
            }
        }

        private async Task HandleKycOkEmailAsync(SendEmailData<KycOkData> result)
        {
            _log.Info(nameof(HandleKycOkEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateWelcomeFxMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleConfirmEmailAsync(SendEmailData<EmailComfirmationData> result)
        {
            _log.Info(nameof(HandleConfirmEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateConfirmEmailMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleCashInEmailAsync(SendEmailData<CashInData> result)
        {
            _log.Info(nameof(HandleCashInEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashInMsg(result.PartnerId, result.MessageData);

            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashOutRequestAsync(SendEmailData<SwiftCashOutRequestData> result)
        {
            _log.Info(nameof(HandleSwiftCashOutRequestAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashOutRequestMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleNoRefundDepositDoneEmailAsync(SendEmailData<NoRefundDepositDoneData> result)
        {
            _log.Info(nameof(HandleNoRefundDepositDoneEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoRefundDepositDoneMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleNoRefundOCashOutEmailAsync(SendEmailData<NoRefundOCashOutData> result)
        {
            _log.Info(nameof(HandleNoRefundOCashOutEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoRefundOCashOutMsg(result.PartnerId, result.MessageData);

            if (msg == null)
            {
                _log.Warning(nameof(HandleNoRefundOCashOutEmailAsync), "Email was not generated");
                return;
            }

            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleBankCashInEmailAsync(SendEmailData<BankCashInData> result)
        {
            _log.Info(nameof(HandleBankCashInEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateBankCashInMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePlainTextBroadcastAsync(SendBroadcastData<PlainTextBroadCastData> result)
        {
            _log.Info(nameof(HandlePlainTextBroadcastAsync), $"Broadcast group: {result.BroadcastGroup}");
            var msg = new EmailMessage
            {
                TextBody = result.MessageData.Text,
                Subject = $"[{result.BroadcastGroup}] {result.MessageData.Subject}"
            };
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleUserRegisteredBroadcastAsync(SendBroadcastData<UserRegisteredData> result)
        {
            _log.Info(nameof(HandleUserRegisteredBroadcastAsync), $"Broadcast group: {result.BroadcastGroup}");
            var personalData = await _personalDataService.GetAsync(result.MessageData.ClientId);
            var msg = await _emailGenerator.GenerateUserRegisteredMsg(result.PartnerId, personalData);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleSwiftConfirmedBroadcastAsync(SendBroadcastData<SwiftConfirmedData> result)
        {
            _log.Info(nameof(HandleSwiftConfirmedBroadcastAsync), $"Broadcast group: {result.BroadcastGroup}");
            var msg = await _emailGenerator.GenerateSwiftConfirmedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleCashInRefundEmailAsync(SendEmailData<CashInRefundData> result)
        {
            _log.Info(nameof(HandleCashInRefundEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashInRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwapRefundEmailAsync(SendEmailData<SwapRefundData> result)
        {
            _log.Info(nameof(HandleSwapRefundEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwapRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleOCashOutRefundEmailAsync(SendEmailData<OrdinaryCashOutRefundData> result)
        {
            _log.Info(nameof(HandleOCashOutRefundEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateOrdinaryCashOutRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleFailedTransactionBroadcastAsync(SendBroadcastData<FailedTransactionData> result)
        {
            _log.Info(nameof(HandleFailedTransactionBroadcastAsync), $"Broadcast group: {result.BroadcastGroup}");
            var msg = _emailGenerator.GenerateFailedTransactionMsg(result.PartnerId, result.MessageData.TransactionId, result.MessageData.AffectedClientIds);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleTransferCompletedEmailAsync(SendEmailData<TransferCompletedData> result)
        {
            _log.Info(nameof(HandleTransferCompletedEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateTransferCompletedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleDirectTransferCompletedEmailAsync(SendEmailData<DirectTransferCompletedData> result)
        {
            _log.Info(nameof(HandleDirectTransferCompletedEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateDirectTransferCompletedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePlainTextEmail(SendEmailData<PlainTextData> result)
        {
            _log.Info(nameof(HandlePlainTextEmail), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = new EmailMessage
            {
                TextBody = result.MessageData.Text,
                HtmlBody = result.MessageData.Text,
                Subject = result.MessageData.Subject
            };
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg, result.MessageData.Sender);
        }

        private async Task HandleMyLykkeCashInEmail(SendEmailData<MyLykkeCashInData> result)
        {
            _log.Info(nameof(HandleMyLykkeCashInEmail), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateMyLykkeCashInMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRemindPasswordEmailAsync(SendEmailData<RemindPasswordData> result)
        {
            _log.Info(nameof(HandleRemindPasswordEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRemindPasswordMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRemindPasswordCypEmailAsync(SendEmailData<RemindPasswordCypData> result)
        {
            _log.Info(nameof(HandleRemindPasswordCypEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRemindPasswordCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        private async Task HandlePrivateWalletAddressEmailAsync(SendEmailData<PrivateWalletAddressData> result)
        {
            _log.Info(nameof(HandlePrivateWalletAddressEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratPrivateWalletAddressMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRestrictedAreaEmailAsync(SendEmailData<RestrictedAreaData> result)
        {
            _log.Info(nameof(HandleRestrictedAreaEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRestrictedAreaMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSolarCashOutCompletedEmailAsync(SendEmailData<SolarCashOutData> result)
        {
            _log.Info(nameof(HandleSolarCashOutCompletedEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratSolarCashOutMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSolarCoinAddressEmailAsync(SendEmailData<SolarCoinAddressData> result)
        {
            _log.Info(nameof(HandleSolarCoinAddressEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratSolarAddressMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleDeclinedDocumentsEmailAsync(SendEmailData<DeclinedDocumentsData> result)
        {
            _log.Info(nameof(HandleDeclinedDocumentsEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateDeclinedDocumentsMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleCashoutUnlockEmailAsync(SendEmailData<CashoutUnlockData> result)
        {
            _log.Info(nameof(HandleCashoutUnlockEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashoutUnlockMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRequestForDocumentEmailAsync(SendEmailData<RequestForDocumentData> result)
        {
            _log.Info(nameof(HandleRequestForDocumentEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRequestForDocumentMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutRequestedAsync(SendEmailData<SwiftCashoutRequestedData> result)
        {
            _log.Info(nameof(HandleSwiftCashoutRequestedAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutRequestedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutProcessedEmailAsync(SendEmailData<SwiftCashoutProcessedData> result)
        {
            _log.Info(nameof(HandleSwiftCashoutProcessedEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutProcessedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutDeclinedEmailAsync(SendEmailData<SwiftCashoutDeclinedData> result)
        {
            _log.Info(nameof(HandleSwiftCashoutDeclinedEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutDeclinedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRegistrationVerifyEmailAsync(SendEmailData<RegistrationEmailVerifyData> result)
        {
            _log.Info(nameof(HandleRegistrationVerifyEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRegistrationVerifyEmailMsgAsync(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRegisteredCypEmailAsync(SendEmailData<RegistrationCypMessageData> result)
        {
            _log.Info(nameof(HandleRegisteredCypEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateWelcomeCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        private async Task HandleKycOkCypEmailAsync(SendEmailData<KycOkCypData> result)
        {
            _log.Info(nameof(HandleKycOkCypEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateWelcomeFxCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        private async Task HandleConfirmCypEmailAsync(SendEmailData<EmailComfirmationCypData> result)
        {
            _log.Info(nameof(HandleConfirmCypEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateConfirmEmailCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        private async Task HandleDirectTransferCompletedCypEmailAsync(SendEmailData<DirectTransferCompletedCypData> result)
        {
            _log.Info(nameof(HandleDirectTransferCompletedCypEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateDirectTransferCompletedCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        private async Task HandleNoAccountPasswordRecoveryEmailAsync(SendEmailData<NoAccountPasswordRecoveryData> result)
        {
            _log.Info(nameof(HandleNoAccountPasswordRecoveryEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoAccountPasswordRecoveryMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleNoAccountPasswordRecoveryCypEmailAsync(SendEmailData<NoAccountPasswordRecoveryCypData> result)
        {
            _log.Info(nameof(HandleNoAccountPasswordRecoveryCypEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoAccountPasswordRecoveryCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutProcessedCypEmailAsync(SendEmailData<SwiftCashoutProcessedCypData> result)
        {
            _log.Info(nameof(HandleSwiftCashoutProcessedCypEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutProcessedCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }
        private async Task HandleSwiftCashoutDeclinedCypEmailAsync(SendEmailData<SwiftCashoutDeclinedCypData> result)
        {
            _log.Info(nameof(HandleSwiftCashoutDeclinedCypEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutDeclinedCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }
        private async Task HandleRejectedCypEmailAsync(SendEmailData<RejectedCypData> result)
        {
            _log.Info(nameof(HandleRejectedCypEmailAsync), $"Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRejectedEmailCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync("LykkeCyprus", result.EmailAddress, msg);
        }

        public void Start()
        {
            foreach (var queueReader in _queueReadersList)
            {
                queueReader.Start();
                _log.Info(nameof(Start), $"Started:{queueReader.GetComponentName()}");
            }
        }
    }
}
