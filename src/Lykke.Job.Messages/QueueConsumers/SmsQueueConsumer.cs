using AzureStorage.Queue;
using Common;
using Common.Log;
using Lykke.Job.Messages.Contract.Sms;
using Lykke.Service.SmsSender.Client;
using Lykke.Service.TemplateFormatter.Client;
using Lykke.Service.TemplateFormatter.TemplateModels;
using System;
using System.Threading.Tasks;
using Autofac;
using Lykke.Common.Log;

namespace Lykke.Job.Messages.QueueConsumers
{
    public class SmsQueueConsumer : IStartable
    {
        private readonly IQueueReader _queueReader;
        private readonly ISmsSenderClient _smsSenderClient;
        private readonly ITemplateFormatter _templateFormatter;
        private readonly ILog _log;

        public SmsQueueConsumer(
            IQueueReader queueReader,
            ISmsSenderClient smsSenderClient,
            ITemplateFormatter templateFormatter,
            ILogFactory logFactory)
        {
            _queueReader = queueReader;
            _smsSenderClient = smsSenderClient;
            _templateFormatter = templateFormatter;
            _log = logFactory.CreateLog(this);

            InitQueues();
        }

        private void InitQueues()
        {
            _queueReader.RegisterPreHandler(data =>
            {
                if (data == null)
                {
                    _log.Info(nameof(InitQueues), "Queue had unknown SMS send request");
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            });

            _queueReader.RegisterHandler<SendSmsData<SmsConfirmationData>>(
                "SmsConfirmMessage", HandleSmsRequestAsync);
            _queueReader.RegisterHandler<SendSmsData<string>>("SimpleSmsMessage", HandleSimpleSmsRequestAsync);

            _log.Info(nameof(InitQueues), $"Registered:{_queueReader.GetComponentName()}");
        }

        private async Task HandleSimpleSmsRequestAsync(SendSmsData<string> request)
        {
            _log.Info(nameof(HandleSimpleSmsRequestAsync),
                $"SMS: {request.MessageData}. Receiver: {request.PhoneNumber.SanitizePhone()}, UTC: {DateTime.UtcNow}");

            await _smsSenderClient.SendSmsAsync(request.PhoneNumber, request.MessageData);
        }

        private async Task HandleSmsRequestAsync(SendSmsData<SmsConfirmationData> request)
        {
            _log.Info(nameof(HandleSmsRequestAsync),
                $"SMS: Phone confirmation. Receiver: {request.PhoneNumber.SanitizePhone()}, UTC: {DateTime.UtcNow}");

            var msgText = await _templateFormatter.FormatAsync(nameof(SmsConfirmationTemplate), request.PartnerId, "EN",
                new SmsConfirmationTemplate
                {
                    ConfirmationCode = request.MessageData.ConfirmationCode
                });

            await _smsSenderClient.SendSmsAsync(request.PhoneNumber, msgText.Subject);
        }

        public void Start()
        {
            _queueReader.Start();
            _log.Info(nameof(Start), $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
