using Common;
using Common.Log;
using Lykke.Job.Messages.Contract.Emails.MessageData;
using Lykke.Job.Messages.Contract.Utils;
using Lykke.Job.Messages.Core.Domain.Email.Models;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Service.EmailSender;
using Lykke.Service.PersonalData.Contract;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Services.Email
{
    public class EmailMessageProcessor : IEmailMessageProcessor
    {
        private Dictionary<string, Type> _emailTemplateTypeDict;
        private Dictionary<Type, object> _registeredMethods;
        private readonly IPersonalDataService _personalDataService;
        private readonly ISmtpEmailSender _smtpEmailSender;
        private readonly ILog _log;
        private readonly IEmailGeneratorNew _emailGenerator;

        public EmailMessageProcessor(IEmailGeneratorNew emailGenerator,
            ISmtpEmailSender smtpEmailSender,
            IPersonalDataService personalDataService,
            ILog log)
        {
            _personalDataService = personalDataService;
            _smtpEmailSender = smtpEmailSender;
            _log = log;
            _emailGenerator = emailGenerator;

            RegisteredMethods();
            RegisterTemplates();
        }

        public async Task SendAsync<T>(SendEmailRequest<T> emailRequest) where T : IEmailMessageData
        {
            var type = emailRequest.MessageData.GetType();

            if (!_registeredMethods.TryGetValue(type, out var func))
                throw new Exception("Email is not registered for sending");

            var converted = new SendEmailData<T>()
            {
                EmailAddress = emailRequest.EmailAddress,
                MessageData = emailRequest.MessageData,
                PartnerId = emailRequest.PartnerId
            };

            await ((Func<SendEmailData<T>, Task>)func)((dynamic)converted);
        }

        public Type GetTypeForTemplateId(string templateId)
        {
            _emailTemplateTypeDict.TryGetValue(templateId, out var type);

            return type;
        }

        private void RegisterTemplates()
        {
            _emailTemplateTypeDict = new Dictionary<string, Type>();
            var emailMessageDataType = typeof(IEmailMessageData);
            var emailTypes = ReflectionUtil.GetImplTypesAssignableToMarkerTypeFromAsssembly(
                                            emailMessageDataType.Assembly,
                                            emailMessageDataType);

            emailTypes.ForEach(type =>
            {
                var templateIdValue = (string)ReflectionUtil.ExtractConstValueFromType(type, "EmailTemplateId");

                if (string.IsNullOrEmpty(templateIdValue))
                {
                    throw new Exception($"All MessageData classes should contain EmailTemplateId const field. Error in {type}");
                }

                if (_emailTemplateTypeDict.TryGetValue(templateIdValue, out var temp))
                {
                    throw new Exception($"Type with EmailTemplateId == {templateIdValue} has been already registered. Error in {type}");
                }

                _emailTemplateTypeDict[templateIdValue] = type;
            });
        }

        //Register methods via reflection
        private void RegisteredMethods()
        {
            _registeredMethods = new Dictionary<Type, object>();
            var methods = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

            methods.Where(x =>
            {
                var parameters = x.GetParameters();
                var parameter1 = parameters.FirstOrDefault();

                if (parameter1 == null)
                    return false;

                if (!parameter1.ParameterType.IsGenericType)
                    return false;

                if (!typeof(Task).IsAssignableFrom(x.ReturnType))
                    return false;

                Type[] typeArguments = parameter1.ParameterType.GetGenericArguments();
                var genericArgument = typeArguments.FirstOrDefault();

                if (genericArgument == null)
                    return false;

                var typeToRegister = typeArguments.FirstOrDefault();

                ParameterExpression parameter = Expression.Parameter(parameter1.ParameterType, "i");
                var delegateType = typeof(Func<,>).MakeGenericType(parameter1.ParameterType, x.ReturnType);
                var yourExpression = Expression.Lambda(delegateType, property, parameter);

                _registeredMethods[typeToRegister] = (Func<SendEmailData<IEmailMessageData>, Task>)(async (y) =>
                {
                    await (Task)x.Invoke(this, new[] { y });
                });

                return true;
            }).ToArray();
        }

        #region EMailGeneration Methods

        private async Task HandleLykkeVisaCardEmailAsync(SendEmailData<LykkeCardVisaData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleLykkeVisaCardEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
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
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleRejectedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                     $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRejectedEmailMsg(result.PartnerId);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRegisteredEmailAsync(SendEmailData<RegistrationMessageData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleRegisteredEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
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
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleKycOkEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                  $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateWelcomeFxMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleConfirmEmailAsync(SendEmailData<EmailComfirmationData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleConfirmEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                    $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateConfirmEmailMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleCashInEmailAsync(SendEmailData<CashInData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleCashInEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                   $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashInMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashOutRequestAsync(SendEmailData<SwiftCashOutRequestData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleSwiftCashOutRequestAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                           $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashOutRequestMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleNoRefundDepositDoneEmailAsync(SendEmailData<NoRefundDepositDoneData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleNoRefundDepositDoneEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoRefundDepositDoneMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleNoRefundOCashOutEmailAsync(SendEmailData<NoRefundOCashOutData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleNoRefundOCashOutEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                             $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateNoRefundOCashOutMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleBankCashInEmailAsync(SendEmailData<BankCashInData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleBankCashInEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                       $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateBankCashInMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePlainTextBroadcastAsync(SendBroadcastData<PlainTextBroadCastData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandlePlainTextBroadcastAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                          $"{Environment.NewLine}Broadcast group: {result.BroadcastGroup}");
            var msg = new EmailMessage
            {
                TextBody = result.MessageData.Text,
                Subject = $"[{result.BroadcastGroup}] {result.MessageData.Subject}"
            };
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, result.BroadcastGroup, msg);
        }

        private async Task HandleUserRegisteredBroadcastAsync(SendBroadcastData<UserRegisteredData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleUserRegisteredBroadcastAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Broadcast group: {result.BroadcastGroup}");
            var personalData = await _personalDataService.GetAsync(result.MessageData.ClientId);
            var msg = await _emailGenerator.GenerateUserRegisteredMsg(result.PartnerId, personalData);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, result.BroadcastGroup, msg);
        }

        private async Task HandleSwiftConfirmedBroadcastAsync(SendBroadcastData<SwiftConfirmedData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleSwiftConfirmedBroadcastAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Broadcast group: {result.BroadcastGroup}");
            var msg = await _emailGenerator.GenerateSwiftConfirmedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, result.BroadcastGroup, msg);
        }

        private async Task HandleCashInRefundEmailAsync(SendEmailData<CashInRefundData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleCashInRefundEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                         $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashInRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwapRefundEmailAsync(SendEmailData<SwapRefundData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleSwapRefundEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                       $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwapRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleOCashOutRefundEmailAsync(SendEmailData<OrdinaryCashOutRefundData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleOCashOutRefundEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                           $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateOrdinaryCashOutRefundMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleFailedTransactionBroadcastAsync(SendBroadcastData<FailedTransactionData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleFailedTransactionBroadcastAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                  $"{Environment.NewLine}Broadcast group: {result.BroadcastGroup}");
            var msg = _emailGenerator.GenerateFailedTransactionMsg(result.PartnerId, result.MessageData.TransactionId, result.MessageData.AffectedClientIds);
            await _smtpEmailSender.SendBroadcastAsync(result.PartnerId, result.BroadcastGroup, msg);
        }

        private async Task HandleTransferCompletedEmailAsync(SendEmailData<TransferCompletedData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleTransferCompletedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                              $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateTransferCompletedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleDirectTransferCompletedEmailAsync(SendEmailData<DirectTransferCompletedData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleDirectTransferCompletedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                    $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateDirectTransferCompletedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePlainTextEmail(SendEmailData<PlainTextData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandlePlainTextEmail), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
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
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleMyLykkeCashInEmail), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                     $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateMyLykkeCashInMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRemindPasswordEmailAsync(SendEmailData<RemindPasswordData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleRemindPasswordEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                           $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRemindPasswordMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandlePrivateWalletAddressEmailAsync(SendEmailData<PrivateWalletAddressData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandlePrivateWalletAddressEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                 $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratPrivateWalletAddressMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSolarCashOutCompletedEmailAsync(SendEmailData<SolarCashOutData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleSolarCashOutCompletedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                                  $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratSolarCashOutMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSolarCoinAddressEmailAsync(SendEmailData<SolarCoinAddressData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleSolarCoinAddressEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                             $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GeneratSolarAddressMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleDeclinedDocumentsEmailAsync(SendEmailData<DeclinedDocumentsData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleDeclinedDocumentsEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                              $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateDeclinedDocumentsMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleCashoutUnlockEmailAsync(SendEmailData<CashoutUnlockData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleCashoutUnlockEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                          $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateCashoutUnlockMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRequestForDocumentEmailAsync(SendEmailData<RequestForDocumentData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleRequestForDocumentEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRequestForDocumentMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutProcessedEmailAsync(SendEmailData<SwiftCashoutProcessedData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleSwiftCashoutProcessedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutProcessedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleSwiftCashoutDeclinedEmailAsync(SendEmailData<SwiftCashoutDeclinedData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleSwiftCashoutDeclinedEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateSwiftCashoutDeclinedMsg(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        private async Task HandleRegistrationVerifyEmailAsync(SendEmailData<RegistrationEmailVerifyData> result)
        {
            await _log.WriteInfoAsync(nameof(EmailMessageProcessor), nameof(HandleRegistrationVerifyEmailAsync), null, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" +
                                                                                                               $"{Environment.NewLine}Email to: {result.EmailAddress.SanitizeEmail()}");
            var msg = await _emailGenerator.GenerateRegistrationVerifyEmailMsgAsync(result.PartnerId, result.MessageData);
            await _smtpEmailSender.SendEmailAsync(result.PartnerId, result.EmailAddress, msg);
        }

        #endregion
    }
}
