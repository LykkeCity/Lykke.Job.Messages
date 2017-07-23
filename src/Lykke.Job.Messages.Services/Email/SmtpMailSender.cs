using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.Messages.Core.Domain.Email;
using Lykke.Job.Messages.Core.Services.Email;
using MailKit.Net.Smtp;
using MimeKit;

namespace Lykke.Job.Messages.Services.Email
{
    public class SmtpMailSender : ISmtpEmailSender
    {
        private readonly ILog _log;
        private readonly Func<SmtpClient> _clientFactory;
        private readonly MailboxAddress _mailFrom;
        private readonly IBroadcastMailsRepository _broadcastMailsRepository;

        public SmtpMailSender(ILog log, Func<SmtpClient> clientFactory, MailboxAddress mailFrom,
            IBroadcastMailsRepository broadcastMailsRepository)
        {
            _log = log;
            _clientFactory = clientFactory;
            _mailFrom = mailFrom;
            _broadcastMailsRepository = broadcastMailsRepository;
        }

        public async Task SendEmailAsync(string emailAddress, EmailMessage msg, string sender = null)
        {
            var to = new MailboxAddress(emailAddress);

            var mailAttachments = msg.Attachments?.Select(
                                      x =>
                                      {
                                          x.Stream.Seek(0, SeekOrigin.Begin);
                                          var att = new MimePart(x.ContentType)
                                          {
                                              ContentObject = new ContentObject(x.Stream),
                                              FileName = x.FileName
                                          };
                                          return att;
                                      }).ToArray() ?? new MimePart[0];

            sender = sender.IsValidEmail() ? sender : null;

            var mailMessage = new MimeMessage();

            mailMessage.From.Add(sender != null ? new MailboxAddress(sender) : _mailFrom);
            mailMessage.To.Add(to);
            mailMessage.Subject = msg.Subject;

            var builder = new BodyBuilder { HtmlBody = msg.Body };
            foreach (var mailAttachment in mailAttachments)
                builder.Attachments.Add(mailAttachment);

            mailMessage.Body = builder.ToMessageBody();

            try
            {
                using (var client = _clientFactory())
                {
                    await client.SendAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                if (_log != null)
                    await _log.WriteWarningAsync("Mail sender", "Send mail", to.Address,
                        ex.Message);
            }

        }

        public async Task SendBroadcastAsync(BroadcastGroup broadcastGroup, EmailMessage message)
        {
            var emails = await _broadcastMailsRepository.GetEmailsByGroup(broadcastGroup);
            foreach (var email in emails)
            {
                await SendEmailAsync(email.Email, message);
            }
        }
    }
}
