using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract.Emails.MessageData;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Workflow.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Workflow.CommandHandlers
{
    public class SendEmailCommandHandler
    {
        private readonly ILog _log;
        private readonly IEmailMessageProcessor _emailMessageProcessor;

        public SendEmailCommandHandler(ILog log,
           IEmailMessageProcessor emailMessageProcessor
            )
        {
            _log = log;
            _emailMessageProcessor = emailMessageProcessor;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle<T>(SendEmailCommand<T> command, IEventPublisher publisher) where T : IEmailMessageData
        {
            _log.WriteInfo(nameof(SendEmailCommandHandler), command, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" + 
                $"{Environment.NewLine}Email to: {command.EmailAddress.SanitizeEmail()}");

            await _emailMessageProcessor.SendAsync(new Core.Domain.Email.Models.SendEmailRequest<T>()
            {
                EmailAddress = command.EmailAddress,
                MessageData = command.MessageData,
                PartnerId = command.PartnerId
            });

            return CommandHandlingResult.Ok();
        }
    }
}
