namespace Lykke.Job.Messages.Core.Domain.Clients
{
    public interface IFullPersonalData : IPersonalData
    {
        string PasswordHint { get; set; }
    }
}