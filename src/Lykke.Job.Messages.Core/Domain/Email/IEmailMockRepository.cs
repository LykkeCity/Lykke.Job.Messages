using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.Email
{
    public interface IEmailMockRepository
    {
        Task<ISmtpMailMock> InsertAsync(string address, EmailMessage msg);
    }
}