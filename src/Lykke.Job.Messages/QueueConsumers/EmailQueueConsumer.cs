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
            _log = log;

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
                        await _log.WriteInfoAsync(nameof(EmailQueueConsumer), "InitQueues", null, "Queue had unknown message");
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
            }
        }

        private async Task HandleLykkeVisaCardEmailAsync(SendEmailData<LykkeCardVisaData> result)
         {
             await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleLykkeVisaCardEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                        $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
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
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleRejectedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                     $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRejectedEmailMsg(result.PartnerId);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRegisteredEmailAsync(SendEmailData<RegistrationMessageData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleRegisteredEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                       $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
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
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleKycOkEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                  $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateWelcomeFxMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleConfirmEmailAsync(SendEmailData<EmailComfirmationData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleConfirmEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                    $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateConfirmEmailMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleCashInEmailAsync(SendEmailData<CashInData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleCashInEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                   $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashInMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashOutRequestAsync(SendEmailData<SwiftCashOutRequestData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleSwiftCashOutRequestAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                           $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashOutRequestMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleNoRefundDepositDoneEmailAsync(SendEmailData<NoRefundDepositDoneData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleNoRefundDepositDoneEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoRefundDepositDoneMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleNoRefundOCashOutEmailAsync(SendEmailData<NoRefundOCashOutData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleNoRefundOCashOutEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                             $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoRefundOCashOutMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleBankCashInEmailAsync(SendEmailData<BankCashInData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleBankCashInEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                       $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateBankCashInMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePlainTextBroadcastAsync(SendBroadcastData<PlainTextBroadCastData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandlePlainTextBroadcastAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                          $"{Environment.NewLine}Broadcast group: {result.BroadcastGroup}");
            var msg = new EmailMessage
            {
                TextBody = result.MessageData.Text,
                Subject = $"[{result.BroadcastGroup}] {result.MessageData.Subject}"
            };
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleUserRegisteredBroadcastAsync(SendBroadcastData<UserRegisteredData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleUserRegisteredBroadcastAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Broadcast group: {result.BroadcastGroup}");
            var personalData = await _personalDataService.GetAsync(result.MessageData.ClientId);
            var msg = await _emailGenerator.GenerateUserRegisteredMsg(result.PartnerId, personalData);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleSwiftConfirmedBroadcastAsync(SendBroadcastData<SwiftConfirmedData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleSwiftConfirmedBroadcastAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Broadcast group: {result.BroadcastGroup}");
            var msg = await _emailGenerator.GenerateSwiftConfirmedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleCashInRefundEmailAsync(SendEmailData<CashInRefundData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleCashInRefundEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                         $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashInRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwapRefundEmailAsync(SendEmailData<SwapRefundData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleSwapRefundEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                       $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwapRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleOCashOutRefundEmailAsync(SendEmailData<OrdinaryCashOutRefundData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleOCashOutRefundEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                           $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateOrdinaryCashOutRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleFailedTransactionBroadcastAsync(SendBroadcastData<FailedTransactionData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleFailedTransactionBroadcastAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                  $"{Environment.NewLine}Broadcast group: {result.BroadcastGroup}");
            var msg = _emailGenerator.GenerateFailedTransactionMsg(result.PartnerId, result.MessageData.TransactionId, result.MessageData.AffectedClientIds);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, (BroadcastGroup)result.BroadcastGroup, msg);
        }

        private async Task HandleTransferCompletedEmailAsync(SendEmailData<TransferCompletedData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleTransferCompletedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                              $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateTransferCompletedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleDirectTransferCompletedEmailAsync(SendEmailData<DirectTransferCompletedData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleDirectTransferCompletedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                    $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateDirectTransferCompletedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePlainTextEmail(SendEmailData<PlainTextData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandlePlainTextEmail), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                 $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = new EmailMessage
            {
                TextBody = result.MessageData.Text,
                Subject = result.MessageData.Subject
            };
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg, result.MessageData.Sender);
        }

        private async Task HandleMyLykkeCashInEmail(SendEmailData<MyLykkeCashInData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleMyLykkeCashInEmail), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                     $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateMyLykkeCashInMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRemindPasswordEmailAsync(SendEmailData<RemindPasswordData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleRemindPasswordEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                           $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRemindPasswordMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePrivateWalletAddressEmailAsync(SendEmailData<PrivateWalletAddressData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandlePrivateWalletAddressEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                 $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratPrivateWalletAddressMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRestrictedAreaEmailAsync(SendEmailData<RestrictedAreaData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleRestrictedAreaEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                 $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRestrictedAreaMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSolarCashOutCompletedEmailAsync(SendEmailData<SolarCashOutData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleSolarCashOutCompletedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                  $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratSolarCashOutMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSolarCoinAddressEmailAsync(SendEmailData<SolarCoinAddressData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleSolarCoinAddressEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                             $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratSolarAddressMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleDeclinedDocumentsEmailAsync(SendEmailData<DeclinedDocumentsData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleDeclinedDocumentsEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                              $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateDeclinedDocumentsMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleCashoutUnlockEmailAsync(SendEmailData<CashoutUnlockData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleCashoutUnlockEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                          $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashoutUnlockMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRequestForDocumentEmailAsync(SendEmailData<RequestForDocumentData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleRequestForDocumentEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRequestForDocumentMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutProcessedEmailAsync(SendEmailData<SwiftCashoutProcessedData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleSwiftCashoutProcessedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutProcessedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutDeclinedEmailAsync(SendEmailData<SwiftCashoutDeclinedData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleSwiftCashoutDeclinedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutDeclinedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }
        
        private async Task HandleRegistrationVerifyEmailAsync(SendEmailData<RegistrationEmailVerifyData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleRegistrationVerifyEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRegistrationVerifyEmailMsgAsync(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRegisteredCypEmailAsync(SendEmailData<RegistrationCypMessageData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleRegisteredCypEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                            $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");

            var msg = await _emailGenerator.GenerateWelcomeCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleKycOkCypEmailAsync(SendEmailData<KycOkCypData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailQueueConsumer), nameof(HandleKycOkCypEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                       $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateWelcomeFxCypMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        public void Start()
        {
            foreach (var queueReader in _queueReadersList)
            {
                queueReader.Start();
                _log.WriteInfoAsync(
                    nameof(Messages),
                    nameof(EmailQueueConsumer),
                    nameof(Start),
                    $"Started:{queueReader.GetComponentName()}")
                    .Wait();
            }
        }
    }
}