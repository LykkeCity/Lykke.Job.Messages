using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Lykke.Job.Messages.Contract.Email;
using Lykke.Job.Messages.Core.Domain.Clients;
using Lykke.Job.Messages.Core.Domain.Email;
using Lykke.Job.Messages.Core.Domain.Email.MessagesData;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Service.Registration.Models.MessagesData;

namespace Lykke.Job.Messages.QueueConsumers
{
    public class EmailQueueConsumer
    {
        private readonly IEnumerable<IQueueReader> _queueReadersList;
        private readonly ISmtpEmailSender _smtpEmailSender;
        private readonly IEmailGenerator _emailGenerator;
        private readonly IPersonalDataRepository _personalDataRepository;
        private readonly ILog _log;

        public EmailQueueConsumer(IEnumerable<IQueueReader> queueReadersList, ISmtpEmailSender smtpEmailSender,
            IEmailGenerator emailGenerator, IPersonalDataRepository personalDataRepository, ILog log)
        {
            _queueReadersList = queueReadersList;
            _smtpEmailSender = smtpEmailSender;
            _emailGenerator = emailGenerator;
            _personalDataRepository = personalDataRepository;
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
                        await _log.WriteInfoAsync("EmailRequestQueueConsumer", "InitQueues", null, "Queue had unknown message");
                        return false;
                    }
                    return true;
                });

                queueReader.RegisterHandler<QueueRequestModel<QueueMessagesData<RegistrationMessageData>>>(
                    new RegistrationMessageData().MessageId(), itm => HandleRegisteredEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<KycOkData>>>(
                    new KycOkData().MessageId(), itm => HandleKycOkEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<EmailComfirmationData>>>(
                    new EmailComfirmationData().MessageId(), itm => HandleConfirmEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<CashInData>>>(
                    new CashInData().MessageId(), itm => HandleCashInEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<NoRefundDepositDoneData>>>(
                    new NoRefundDepositDoneData().MessageId(), itm => HandleNoRefundDepositDoneEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<NoRefundOCashOutData>>>(
                    new NoRefundOCashOutData().MessageId(), itm => HandleNoRefundOCashOutEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<BankCashInData>>>(
                    new BankCashInData().MessageId(), itm => HandleBankCashInEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SwiftCashOutRequestData>>>(
                    new SwiftCashOutRequestData().MessageId(), itm => HandleSwiftCashOutRequestAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RejectedData>>>(
                    new RejectedData().MessageId(), itm => HandleRejectedEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<CashInRefundData>>>(
                    new CashInRefundData().MessageId(), itm => HandleCashInRefundEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SwapRefundData>>>(
                    new SwapRefundData().MessageId(), itm => HandleSwapRefundEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<OrdinaryCashOutRefundData>>>(
                    new OrdinaryCashOutRefundData().MessageId(), itm => HandleOCashOutRefundEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<TransferCompletedData>>>(
                    new TransferCompletedData().MessageId(), itm => HandleTransferCompletedEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<DirectTransferCompletedData>>>(
                    new DirectTransferCompletedData().MessageId(), itm => HandleDirectTransferCompletedEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<PlainTextData>>>(
                    new PlainTextData().MessageId(), itm => HandlePlainTextEmail(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<MyLykkeCashInData>>>(
                    new MyLykkeCashInData().MessageId(), itm => HandleMyLykkeCashInEmail(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RemindPasswordData>>>(
                    new RemindPasswordData().MessageId(), itm => HandleRemindPasswordEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendBroadcastData<UserRegisteredData>>>(
                    new UserRegisteredData().MessageId(), itm => HandleUserRegisteredBroadcastAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendBroadcastData<SwiftConfirmedData>>>(
                    new SwiftConfirmedData().MessageId(), itm => HandleSwiftConfirmedBroadcastAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendBroadcastData<PlainTextBroadCastData>>>(
                    new PlainTextBroadCastData().MessageId(), itm => HandlePlainTextBroadcastAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendBroadcastData<FailedTransactionData>>>(
                    new FailedTransactionData().MessageId(), itm => HandleFailedTransactionBroadcastAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<PrivateWalletAddressData>>>(
                    new PrivateWalletAddressData().MessageId(), itm => HandlePrivateWalletAddressEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SolarCashOutData>>>(
                    new SolarCashOutData().MessageId(), itm => HandleSolarCashOutCompletedEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<SolarCoinAddressData>>>(
                    new SolarCoinAddressData().MessageId(), itm => HandleSolarCoinAddressEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<DeclinedDocumentsData>>>(
                    new DeclinedDocumentsData().MessageId(), itm => HandleDeclinedDocumentsEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<CashoutUnlockData>>>(
                    new CashoutUnlockData().MessageId(), itm => HandleCashoutUnlockEmailAsync(itm.Data));

                queueReader.RegisterHandler<QueueRequestModel<SendEmailData<RequestForDocumentData>>>(
                    new RequestForDocumentData().MessageId(), itm => HandleRequestForDocumentEmailAsync(itm.Data));
            }
        }

        private async Task HandleRejectedEmailAsync(SendEmailData<RejectedData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleRejectedEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                     $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateRejectedEmailMsg();
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }


        private async Task HandleRegisteredEmailAsync(QueueMessagesData<RegistrationMessageData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleRegisteredEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                       $"{Environment.NewLine}{result.ToJson()}");
            var registerData = new RegistrationData
            {
                ClientId = result.MessageData.ClientId,
                Year = result.MessageData.Year
            };

            var msg = await _emailGenerator.GenerateWelcomeMsg(registerData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleKycOkEmailAsync(SendEmailData<KycOkData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleKycOkEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                  $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateWelcomeFxMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleConfirmEmailAsync(SendEmailData<EmailComfirmationData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleConfirmEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                    $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateConfirmEmailMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleCashInEmailAsync(SendEmailData<CashInData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleCashInEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                   $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateCashInMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashOutRequestAsync(SendEmailData<SwiftCashOutRequestData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleSwiftCashOutRequestAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                           $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateSwiftCashOutRequestMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleNoRefundDepositDoneEmailAsync(SendEmailData<NoRefundDepositDoneData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleNoRefundDepositDoneEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateNoRefundDepositDoneMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleNoRefundOCashOutEmailAsync(SendEmailData<NoRefundOCashOutData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleNoRefundOCashOutEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                             $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateNoRefundOCashOutMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleBankCashInEmailAsync(SendEmailData<BankCashInData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleBankCashInEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                       $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateBankCashInMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandlePlainTextBroadcastAsync(SendBroadcastData<PlainTextBroadCastData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandlePlainTextBroadcastAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                          $"{Environment.NewLine}{result.ToJson()}");
            var msg = new EmailMessage
            {
                Body = result.MessageData.Text,
                IsHtml = false,
                Subject = $"[{result.BroadcastGroup}] {result.MessageData.Subject}"
            };
            await _smtpEmailSender.SendBroadcastAsync(result.BroadcastGroup, msg);
        }

        private async Task HandleUserRegisteredBroadcastAsync(SendBroadcastData<UserRegisteredData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleUserRegisteredBroadcastAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}{result.ToJson()}");
            var personalData = await _personalDataRepository.GetAsync(result.MessageData.ClientId);
            var msg = await _emailGenerator.GenerateUserRegisteredMsg(personalData);
            await _smtpEmailSender.SendBroadcastAsync(result.BroadcastGroup, msg);
        }

        private async Task HandleSwiftConfirmedBroadcastAsync(SendBroadcastData<SwiftConfirmedData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleSwiftConfirmedBroadcastAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateSwiftConfirmedMsg(result.MessageData);
            await _smtpEmailSender.SendBroadcastAsync(result.BroadcastGroup, msg);
        }

        private async Task HandleCashInRefundEmailAsync(SendEmailData<CashInRefundData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleCashInRefundEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                         $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateCashInRefundMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleSwapRefundEmailAsync(SendEmailData<SwapRefundData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleSwapRefundEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                       $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateSwapRefundMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleOCashOutRefundEmailAsync(SendEmailData<OrdinaryCashOutRefundData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleOCashOutRefundEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                           $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateOrdinaryCashOutRefundMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleFailedTransactionBroadcastAsync(SendBroadcastData<FailedTransactionData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleFailedTransactionBroadcastAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                  $"{Environment.NewLine}{result.ToJson()}");
            var msg = _emailGenerator.GenerateFailedTransactionMsg(result.MessageData.TransactionId, result.MessageData.AffectedClientIds);
            await _smtpEmailSender.SendBroadcastAsync(result.BroadcastGroup, msg);
        }

        private async Task HandleTransferCompletedEmailAsync(SendEmailData<TransferCompletedData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleTransferCompletedEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                              $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateTransferCompletedMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleDirectTransferCompletedEmailAsync(SendEmailData<DirectTransferCompletedData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleDirectTransferCompletedEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                    $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateDirectTransferCompletedMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandlePlainTextEmail(SendEmailData<PlainTextData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandlePlainTextEmail", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                 $"{Environment.NewLine}{result.ToJson()}");
            var msg = new EmailMessage
            {
                Body = result.MessageData.Text,
                IsHtml = false,
                Subject = result.MessageData.Subject
            };
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg, result.MessageData.Sender);
        }

        private async Task HandleMyLykkeCashInEmail(SendEmailData<MyLykkeCashInData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleMyLykkeCashInEmail", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                     $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateMyLykkeCashInMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleRemindPasswordEmailAsync(SendEmailData<RemindPasswordData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleRemindPasswordEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                           $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateRemindPasswordMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandlePrivateWalletAddressEmailAsync(SendEmailData<PrivateWalletAddressData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandlePrivateWalletAddressEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                 $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GeneratPrivateWalletAddressMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleSolarCashOutCompletedEmailAsync(SendEmailData<SolarCashOutData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleSolarCashOutCompletedEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                  $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GeneratSolarCashOutMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleSolarCoinAddressEmailAsync(SendEmailData<SolarCoinAddressData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleSolarCoinAddressEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                             $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GeneratSolarAddressMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleDeclinedDocumentsEmailAsync(SendEmailData<DeclinedDocumentsData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleDeclinedDocumentsEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                              $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateDeclinedDocumentsMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleCashoutUnlockEmailAsync(SendEmailData<CashoutUnlockData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleCashoutUnlockEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                          $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateCashoutUnlockMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        private async Task HandleRequestForDocumentEmailAsync(SendEmailData<RequestForDocumentData> result)
        {
            await _log.WriteInfoAsync("EmailRequestQueueConsumer", "HandleRequestForDocumentEmailAsync", null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}{result.ToJson()}");
            var msg = await _emailGenerator.GenerateRequestForDocumentMsg(result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.EmailAddress, msg);
        }

        public void Start()
        {
            foreach (var queueReader in _queueReadersList)
            {
                queueReader.Start();
                Console.WriteLine($"Started:{queueReader.GetComponentName()}");
            }
        }
    }
}