using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Services.Email.TemplateModels
{
    public class SwiftCashoutProcessedTemplate
    {
        public string AssetId { get; set; }
        public double Amount { get; set; }
        public string FullName { get; set; }
        public string Bic { get; set; }
        public string AccNum { get; set; }
        public string AccName { get; set; }
        public string BankName { get; set; }
        public string AccHolderAddress { get; set; }
        public int Year { get; internal set; }
    }
}
