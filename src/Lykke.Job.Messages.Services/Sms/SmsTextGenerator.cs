using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Services.Sms;
using Lykke.Job.Messages.Core.Services.Templates;
using Lykke.Job.Messages.Services.Sms.Templates.ViewModels;

namespace Lykke.Job.Messages.Services.Sms
{
    public class SmsTextGenerator : ISmsTextGenerator
    {
        private readonly ITemplateGenerator _templateGenerator;

        public SmsTextGenerator(ITemplateGenerator templateGenerator)
        {
            _templateGenerator = templateGenerator;
        }

        public async Task<string> GenerateConfirmSmsText(string confirmationCode)
        {
            var templateVm = new SmsConfirmationTemplate
            {
                ConfirmationCode = confirmationCode
            };

            return await _templateGenerator.GenerateAsync("SmsConfirmation", templateVm);
        }
    }
}