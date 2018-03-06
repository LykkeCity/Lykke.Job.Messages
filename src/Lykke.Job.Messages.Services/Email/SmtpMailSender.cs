using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.Messages.Contract.Emails;
using Lykke.Job.Messages.Core.Domain.Email;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Service.EmailPartnerRouter;
using Lykke.Service.EmailSender;

namespace Lykke.Job.Messages.Services.Email
{
    public class SmtpMailSender : ISmtpEmailSender
    {
        private readonly ILog _log;
        private readonly IEmailPartnerRouter _emailPartnerRouter;
        private readonly IBroadcastMailsRepository _broadcastMailsRepository;

        public SmtpMailSender(ILog log, IEmailPartnerRouter emailPartnerRouter,
            IBroadcastMailsRepository broadcastMailsRepository)
        {
            _log = log;
            _emailPartnerRouter = emailPartnerRouter;
            _broadcastMailsRepository = broadcastMailsRepository;
        }

        public async Task SendEmailAsync(string partnerId, string emailAddress, EmailMessage message, string sender = null)
        {
            try
            {
                await _emailPartnerRouter.SendAsync(partnerId, message, new EmailAddressee { EmailAddress = emailAddress });
            }
            catch (Exception ex)
            {
                if (_log != null)
                    await _log.WriteWarningAsync("Mail sender", "Send mail", emailAddress.SanitizeEmail(), ex.Message);
            }
        }

        public async Task SendBroadcastAsync(string partnerId, BroadcastGroup broadcastGroup, EmailMessage message)
        {
            var emails = await _broadcastMailsRepository.GetEmailsByGroup(broadcastGroup);

            foreach (var email in emails)
            {
                await SendEmailAsync(partnerId, email.Email, message);
            }
        }
    }
}
