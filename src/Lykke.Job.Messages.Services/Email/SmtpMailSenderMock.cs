using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Email;
using Lykke.Job.Messages.Core.Services.Email;

namespace Lykke.Job.Messages.Services.Email
{
    public class SmtpMailSenderMock : ISmtpEmailSender
    {
        private readonly IEmailMockRepository _emailMockRepository;

        private readonly IBroadcastMailsRepository _broadcastMailsRepository;
        private readonly IAttachmentFileRepository _attachmentFileRepository;
        private readonly IEmailAttachmentsMockRepository _emailAttachmentsMockRepository;

        public SmtpMailSenderMock(
            IEmailMockRepository emailMockRepository,
            IBroadcastMailsRepository broadcastMailsRepository,
            IAttachmentFileRepository attachmentFileRepository,
            IEmailAttachmentsMockRepository emailAttachmentsMockRepository)
        {
            _emailMockRepository = emailMockRepository;
            _broadcastMailsRepository = broadcastMailsRepository;
            _attachmentFileRepository = attachmentFileRepository;
            _emailAttachmentsMockRepository = emailAttachmentsMockRepository;
        }

        public async Task SendEmailAsync(string emailAddress, EmailMessage msg, string sender = null)
        {
            var updatedMock = await _emailMockRepository.InsertAsync(emailAddress, msg);
            if (msg.Attachments != null && msg.Attachments.Any())
            {
                foreach (var att in msg.Attachments)
                {
                    att.Stream.Position = 0;
                    var fileId = await _attachmentFileRepository.InsertAttachment(att.Stream);
                    await _emailAttachmentsMockRepository.InsertAsync(updatedMock.Id, fileId,
                        att.FileName, att.ContentType);
                }
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