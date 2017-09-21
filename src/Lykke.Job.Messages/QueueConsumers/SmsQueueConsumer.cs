using System;
using System.Threading.Tasks;
using Common.Log;
using AzureStorage.Queue;
using Lykke.Job.Messages.Contract.Sms;
using Lykke.Job.Messages.Core.Domain.Sms;
using Lykke.Job.Messages.Core.Services.Sms;

namespace Lykke.Job.Messages.QueueConsumers
{
    public class SmsQueueConsumer
    {
        private readonly IQueueReader _queueReader;
        private readonly ISmsSender _smsSender;
        private readonly ISmsTextGenerator _smsTextGenerator;
        private readonly IAlternativeSmsSender _alternativeSmsSender;
        private readonly ILog _log;

        public SmsQueueConsumer(
            IQueueReader queueReader,
            ISmsSender emailSender,
            ISmsTextGenerator smsTextGenerator,
            IAlternativeSmsSender alternativeSmsSender,
            ILog log)
        {
            _queueReader = queueReader;
            _smsSender = emailSender;
            _smsTextGenerator = smsTextGenerator;
            _alternativeSmsSender = alternativeSmsSender;
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
                $"SMS: {request.MessageData}. Receiver: {request.PhoneNumber}, UTC: {DateTime.UtcNow}");

            var sender = GetSender(request.UseAlternativeProvider);

            await sender.ProcessSmsAsync(request.PhoneNumber, SmsMessage.Create(sender.GetSenderNumber(request.PhoneNumber), request.MessageData));
        }

        private async Task HandleSmsRequestAsync(SendSmsData<SmsConfirmationData> request)
        {
            await _log.WriteInfoAsync(
                nameof(Messages),
                nameof(SmsQueueConsumer),
                nameof(HandleSmsRequestAsync),
                $"SMS: Phone confirmation. Receiver: {request.PhoneNumber}, UTC: {DateTime.UtcNow}");

            var msgText = await _smsTextGenerator.GenerateConfirmSmsText(request.MessageData.ConfirmationCode);
            var sender = GetSender(request.UseAlternativeProvider);

            await sender.ProcessSmsAsync(request.PhoneNumber, SmsMessage.Create(sender.GetSenderNumber(request.PhoneNumber), msgText));
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