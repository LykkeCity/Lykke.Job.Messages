using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Clients;
using Lykke.Job.Messages.Core.Domain.Email;
using Lykke.Job.Messages.Core.Domain.Email.MessagesData;

namespace Lykke.Job.Messages.Core.Services.Email
{
    public interface IEmailGenerator
    {
        Task<EmailMessage> GenerateWelcomeMsg(RegistrationData kycOkData);
        Task<EmailMessage> GenerateWelcomeFxMsg(KycOkData kycOkData);
        Task<EmailMessage> GenerateConfirmEmailMsg(EmailComfirmationData registrationData);
        Task<EmailMessage> GenerateCashInMsg(CashInData messageData);
        Task<EmailMessage> GenerateNoRefundDepositDoneMsg(NoRefundDepositDoneData messageData);
        Task<EmailMessage> GenerateNoRefundOCashOutMsg(NoRefundOCashOutData messageData);
        Task<EmailMessage> GenerateBankCashInMsg(BankCashInData messageData);
        Task<EmailMessage> GenerateCashInRefundMsg(CashInRefundData messageData);
        Task<EmailMessage> GenerateUserRegisteredMsg(IPersonalData messageData);
        Task<EmailMessage> GenerateRejectedEmailMsg();
        EmailMessage GenerateFailedTransactionMsg(string transactionId, string[] clientIds);
        Task<EmailMessage> GenerateSwapRefundMsg(SwapRefundData messageData);
        Task<EmailMessage> GenerateOrdinaryCashOutRefundMsg(OrdinaryCashOutRefundData messageData);
        Task<EmailMessage> GenerateTransferCompletedMsg(TransferCompletedData transferCompletedData);
        Task<EmailMessage> GenerateDirectTransferCompletedMsg(DirectTransferCompletedData transferCompletedData);
        Task<EmailMessage> GenerateMyLykkeCashInMsg(MyLykkeCashInData messageData);
        Task<EmailMessage> GenerateRemindPasswordMsg(RemindPasswordData messageData);
        Task<EmailMessage> GeneratPrivateWalletAddressMsg(PrivateWalletAddressData messageData);
        Task<EmailMessage> GeneratSolarCashOutMsg(SolarCashOutData messageData);
        Task<EmailMessage> GeneratSolarAddressMsg(SolarCoinAddressData messageData);
        Task<EmailMessage> GenerateDeclinedDocumentsMsg(DeclinedDocumentsData messageData);
        Task<EmailMessage> GenerateCashoutUnlockMsg(CashoutUnlockData messageData);
        Task<EmailMessage> GenerateSwiftConfirmedMsg(SwiftConfirmedData messageData);
        Task<EmailMessage> GenerateSwiftCashOutRequestMsg(SwiftCashOutRequestData messageData);
        Task<EmailMessage> GenerateRequestForDocumentMsg(RequestForDocumentData messageData);
        Task<EmailMessage> GenerateSwiftCashoutProcessedMsg(SwiftCashoutProcessedData messageData);
    }
}