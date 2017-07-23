namespace Lykke.Job.Messages.Core.Domain.Email
{
    public interface IBroadcastMail
    {
        string Email { get; }
        BroadcastGroup Group { get; }
    }
}