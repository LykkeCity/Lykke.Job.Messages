using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.Messages.Contract;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PayAuth.Contract.Events;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;

namespace Lykke.Job.Messages.Sagas
{
    [UsedImplicitly]
    public class LykkePayOperationsSaga
    {
        private readonly IPayInvoiceClient _payInvoiceClient;

        private const string EmailApplicationId = "LykkePay"; 

        public LykkePayOperationsSaga([NotNull] IPayInvoiceClient payInvoiceClient)
        {
            _payInvoiceClient = payInvoiceClient ?? throw new ArgumentNullException(nameof(payInvoiceClient));
        }

        [UsedImplicitly]
        public async Task Handle(EmployeeRegistrationCompletedEvent evt, ICommandSender sender)
        {
            EmployeeModel employee = await _payInvoiceClient.GetEmployeeAsync(evt.Id);

            sender.SendCommand(new SendEmailCommand
            {
                ApplicationId = EmailApplicationId,
                EmailAddresses = new [] {employee.Email},
                Template = "lykkepay_employee_registration",
                Payload = new Dictionary<string, string>
                {
                    {"UserName", $"{employee.FirstName} {employee.LastName}"},
                    {"ResetPasswordUrl", evt.ResetPasswordUrl},
                    {"Year", DateTime.Today.Year.ToString()}
                }
            }, EmailMessagesBoundedContext.Name);
        }

        [UsedImplicitly]
        public async Task Handle(EmployeeUpdateCompletedEvent evt, ICommandSender sender)
        {
            if (string.IsNullOrEmpty(evt.ResetPasswordUrl))
                return;

            EmployeeModel employee = await _payInvoiceClient.GetEmployeeAsync(evt.Id);

            sender.SendCommand(new SendEmailCommand
            {
                ApplicationId = EmailApplicationId,
                EmailAddresses = new [] {employee.Email},
                Template = "lykkepay_password_reset",
                Payload = new Dictionary<string, string>
                {
                    {"UserName", $"{employee.FirstName} {employee.LastName}"},
                    {"ResetPasswordUrl", evt.ResetPasswordUrl},
                    {"Year", DateTime.Today.Year.ToString()}
                }
            }, EmailMessagesBoundedContext.Name);
        }
    }
}