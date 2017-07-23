using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Services.Sms
{
    public interface ISmsTextGenerator
    {
        Task<string> GenerateConfirmSmsText(string confirmationCode);
    }
}