namespace Lykke.Job.Messages.Services.Email.TemplateModels
{
    public class PrivateWalletBackupTemplate
    {
        public string WalletName { get; set; }
        public string WalletAddress { get; set; }
        /// <summary>
        /// QR contains json with encoded key(with answer for sec. question) and sec. question
        /// </summary>
        public string QrCodeUrl { get; set; }
        public string SecurityQuestion { get; set; }
        public string Year { get; set; }
    }
}
