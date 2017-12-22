using System;
using System.Threading.Tasks;
using Common.Log;
using AzureStorage.Queue;
using Common;
using Common.PasswordTools;
using Lykke.Job.Messages.Contract.Sms;
using Lykke.Job.Messages.Core.Domain.Sms;
using Lykke.Job.Messages.Core.Services.Sms;
using Lykke.Service.TemplateFormatter;
using Lykke.Service.TemplateFormatter.TemplateModels;

namespace Lykke.Job.Messages.QueueConsumers
{
    public class SmsQueueConsumer
    {
        private readonly IQueueReader _queueReader;
        private readonly ISmsSender _smsSender;
        private readonly IAlternativeSmsSender _alternativeSmsSender;
        private readonly ITemplateFormatter _templateFormatter;
        private readonly ILog _log;

        public SmsQueueConsumer(
            IQueueReader queueReader,
            ISmsSender emailSender,
            IAlternativeSmsSender alternativeSmsSender,
            ITemplateFormatter templateFormatter,
            ILog log)
        {
            _queueReader = queueReader;
            _smsSender = emailSender;
            _alternativeSmsSender = alternativeSmsSender;
            _templateFormatter = templateFormatter;
            _log = log;

            InitQueues();
        }

        private void InitQueues()
        {
            _queueReader.RegisterPreHandler(data =>
            {
                if (data == null)
                {
                    _log.WriteWarningAsync(
                        nameof(Messages),
                        nameof(SmsQueueConsumer),
                        nameof(InitQueues),
                        "Queue had unknown SMS send request")
                        .Wait();
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            });

            _queueReader.RegisterHandler<SendSmsData<SmsConfirmationData>>(
                "SmsConfirmMessage", HandleSmsRequestAsync);
            _queueReader.RegisterHandler<SendSmsData<string>>("SimpleSmsMessage", HandleSimpleSmsRequestAsync);

            _log.WriteInfoAsync(
                nameof(Messages),
                nameof(SmsQueueConsumer),
                nameof(InitQueues),
                $"Registered:{_queueReader.GetComponentName()}")
                .Wait();
        }

        private async Task HandleSimpleSmsRequestAsync(SendSmsData<string> request)
        {
            await _log.WriteInfoAsync(
                nameof(Messages),
                nameof(SmsQueueConsumer),
                nameof(HandleSimpleSmsRequestAsync),
                $"SMS: {request.MessageData}. Receiver: {request.PhoneNumber.SanitizePhone()}, UTC: {DateTime.UtcNow}");

            var sender = GetSender(request.UseAlternativeProvider);

            await sender.ProcessSmsAsync(request.PhoneNumber, SmsMessage.Create(sender.GetSenderNumber(request.PhoneNumber), request.MessageData));
        }

        private async Task HandleSmsRequestAsync(SendSmsData<SmsConfirmationData> request)
        {
            await _log.WriteInfoAsync(
                nameof(Messages),
                nameof(SmsQueueConsumer),
                nameof(HandleSmsRequestAsync),
                $"SMS: Phone confirmation. Receiver: {request.PhoneNumber.SanitizePhone()}, UTC: {DateTime.UtcNow}");

            var msgText = await _templateFormatter.FormatAsync(nameof(SmsConfirmationTemplate), request.PartnerId, "EN",
                new SmsConfirmationTemplate
                {
                    ConfirmationCode = request.MessageData.ConfirmationCode
                });
            var sender = GetSender(request.UseAlternativeProvider);

            await sender.ProcessSmsAsync(request.PhoneNumber, SmsMessage.Create(sender.GetSenderNumber(request.PhoneNumber), msgText.Subject));
        }

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfoAsync(
                nameof(Messages),
                nameof(SmsQueueConsumer),
                nameof(Start),
                $"Started:{_queueReader.GetComponentName()}")
                .Wait();
        }

        private ISmsSender GetSender(bool useAlternative)
        {
            return useAlternative ? _alternativeSmsSender : _smsSender;
        }
    }
}