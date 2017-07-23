using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Services.Templates
{
    public interface IRemoteTemplateGenerator
    {
        Task<string> GenerateAsync<T>(string templateName, T templateVm);
    }
}