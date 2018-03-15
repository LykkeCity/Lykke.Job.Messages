using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Cqrs;
using Lykke.Job.Messages.Commands;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Modules;
using Lykke.Service.EmailSender;

namespace Lykke.Job.Messages.Handlers
{
    public class EmailNotificationsCommandHandler
    {
        private readonly ITemplateFormatter _templateFormatter;
        private readonly ISmtpEmailSender _smtpEmailSender;

        public EmailNotificationsCommandHandler(ITemplateFormatter templateFormatter, ISmtpEmailSender smtpEmailSender)
        {
            _templateFormatter = templateFormatter;
            _smtpEmailSender = smtpEmailSender;
        }

        public async Task<CommandHandlingResult> Handle(SendEmailCommand command)
        {
            await _smtpEmailSender.SendEmailAsync(command.PartnerId, command.EmailAddress, command.Message);

            return CommandHandlingResult.Ok();
        }
    }
}