using System.Threading.Tasks;
using Lykke.Sdk;

namespace Lykke.Job.Messages
{
    class Program
    {
        static async Task Main(string[] args)
        {
#if DEBUG
            await LykkeStarter.Start<Startup>(true);
#else
            await LykkeStarter.Start<Startup>(false);
#endif
        }
    }
}
