using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract.Emails;
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
        private readonly ISmtpEmailSender _smtpEmailSender;

        public SendEmailCommandHandler(ILog log,
            ISmtpEmailSender smtpEmailSender)
        {
            _log = log;
            _smtpEmailSender = smtpEmailSender;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle<T>(SendEmailCommand<T> command, IEventPublisher publisher) where T : IEmailMessageData
        {
            _log.WriteInfo(nameof(SendEmailCommandHandler), command, $"DT: {DateTime.UtcNow.ToIsoDateTime()}" + 
                $"{Environment.NewLine}Email to: {command.EmailAddress.SanitizeEmail()}");

            var msg = await _emailGenerator.GenerateRegistrationVerifyEmailMsgAsync(command.PartnerId, command.MessageData);
            await _smtpEmailSender.SendEmailAsync(command.PartnerId, command.EmailAddress, msg);

            return CommandHandlingResult.Ok();
        }
    }
}
