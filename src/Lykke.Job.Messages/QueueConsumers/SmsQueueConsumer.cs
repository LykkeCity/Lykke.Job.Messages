using System;
using System.Threading.Tasks;
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

        public SmsQueueConsumer(IQueueReader queueReader, ISmsSender emailSender, ISmsTextGenerator smsTextGenerator, IAlternativeSmsSender alternativeSmsSender)
        {
            _queueReader = queueReader;
            _smsSender = emailSender;
            _smsTextGenerator = smsTextGenerator;
            _alternativeSmsSender = alternativeSmsSender;

            InitQueues();
        }

        private void InitQueues()
        {
            _queueReader.RegisterPreHandler(data =>
            {
                if (data == null)
                {
                    Console.WriteLine("Queue had unknown SMS send request");
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            });

            _queueReader.RegisterHandler<SendSmsData<SmsConfirmationData>>(
                "SmsConfirmMessage", HandleSmsRequestAsync);
            _queueReader.RegisterHandler<SendSmsData<string>>("SimpleSmsMessage", HandleSimpleSmsRequestAsync);

            Console.WriteLine($"Registered:{_queueReader.GetComponentName()}");
        }

        private async Task HandleSimpleSmsRequestAsync(SendSmsData<string> request)
        {
            Console.WriteLine($"SMS: {request.MessageData}. Receiver: {request.PhoneNumber}, UTC: {DateTime.UtcNow}");

            var sender = GetSender(request.UseAlternativeProvider);

            await sender.ProcessSmsAsync(request.PhoneNumber, SmsMessage.Create(sender.GetSenderNumber(request.PhoneNumber), request.MessageData));
        }

        private async Task HandleSmsRequestAsync(SendSmsData<SmsConfirmationData> request)
        {
            Console.WriteLine($"SMS: Phone confirmation. Receiver: {request.PhoneNumber}, UTC: {DateTime.UtcNow}");

            var msgText = await _smsTextGenerator.GenerateConfirmSmsText(request.MessageData.ConfirmationCode);
            var sender = GetSender(request.UseAlternativeProvider);

            await sender.ProcessSmsAsync(request.PhoneNumber, SmsMessage.Create(sender.GetSenderNumber(request.PhoneNumber), msgText));
        }

        public void Start()
        {
            _queueReader.Start();
            Console.WriteLine($"Started:{_queueReader.GetComponentName()}");
        }

        private ISmsSender GetSender(bool useAlternative)
        {
            return useAlternative ? _alternativeSmsSender : _smsSender;
        }
    }
}