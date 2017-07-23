namespace Lykke.Job.Messages.Core.Domain.Email
{
    public interface IEmailAttachmentsMock
    {
        string EmailMockId { get; }
        string AttachmentFileId { get; }
        string FileName { get; }
        string ContentType { get; }
    }
}