namespace Lykke.Job.Messages.Core.Services
{
    public interface IHealthService
    {
        string GetHealthViolationMessage();
        string GetHealthWarningMessage();
    }
}