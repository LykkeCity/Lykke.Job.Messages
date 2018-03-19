using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Job.Messages.Commands;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Modules;
using Lykke.Service.EmailSender;

namespace Lykke.Job.Messages.Handlers
{
    public class EmailNotificationsCommandHandler
    {
        private readonly ISmtpEmailSender _smtpEmailSender;
        private readonly ILog _log;
        private readonly TimeSpan _retryPeriodInMinutes;

        public EmailNotificationsCommandHandler(ISmtpEmailSender smtpEmailSender, ILog log, int retryPeriodForEmailsInMinutes)
        {
            _smtpEmailSender = smtpEmailSender;
            _log = log.CreateComponentScope(nameof(EmailNotificationsCommandHandler));
            _retryPeriodInMinutes = TimeSpan.FromMinutes(retryPeriodForEmailsInMinutes);
        }

        public async Task<CommandHandlingResult> Handle(SendEmailCommand command)
        {
            try
            {
                await _smtpEmailSender.SendEmailAsync(command.PartnerId, command.EmailAddress, command.Message);
            }
            catch (Exception e)
            {
                _log.WriteError(nameof(SendEmailCommand), command, e);
                if (DateTime.UtcNow - command.CreationDate < _retryPeriodInMinutes)
                    throw;
            }

            return CommandHandlingResult.Ok();
        }
    }
}