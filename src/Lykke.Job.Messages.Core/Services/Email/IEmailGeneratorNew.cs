using Lykke.Service.EmailSender;
using Lykke.Service.PersonalData.Contract.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Messages.Email.MessageData;
using BankCashInData = Lykke.Job.Messages.Contract.Emails.MessageData.BankCashInData;
using CashInRefundData = Lykke.Job.Messages.Contract.Emails.MessageData.CashInRefundData;
using CashoutUnlockData = Lykke.Job.Messages.Contract.Emails.MessageData.CashoutUnlockData;
using DeclinedDocumentsData = Lykke.Job.Messages.Contract.Emails.MessageData.DeclinedDocumentsData;
using DirectTransferCompletedData = Lykke.Job.Messages.Contract.Emails.MessageData.DirectTransferCompletedData;
using EmailComfirmationData = Lykke.Job.Messages.Contract.Emails.MessageData.EmailComfirmationData;
using KycOkData = Lykke.Job.Messages.Contract.Emails.MessageData.KycOkData;
using LykkeCardVisaData = Lykke.Job.Messages.Contract.Emails.MessageData.LykkeCardVisaData;
using MyLykkeCashInData = Lykke.Job.Messages.Contract.Emails.MessageData.MyLykkeCashInData;
using NoRefundDepositDoneData = Lykke.Job.Messages.Contract.Emails.MessageData.NoRefundDepositDoneData;
using NoRefundOCashOutData = Lykke.Job.Messages.Contract.Emails.MessageData.NoRefundOCashOutData;
using OrdinaryCashOutRefundData = Lykke.Job.Messages.Contract.Emails.MessageData.OrdinaryCashOutRefundData;
using PrivateWalletAddressData = Lykke.Job.Messages.Contract.Emails.MessageData.PrivateWalletAddressData;
using RegistrationEmailVerifyData = Lykke.Job.Messages.Contract.Emails.MessageData.RegistrationEmailVerifyData;
using RegistrationMessageData = Lykke.Job.Messages.Contract.Emails.MessageData.RegistrationMessageData;
using RemindPasswordData = Lykke.Job.Messages.Contract.Emails.MessageData.RemindPasswordData;
using RequestForDocumentData = Lykke.Job.Messages.Contract.Emails.MessageData.RequestForDocumentData;
using SolarCashOutData = Lykke.Job.Messages.Contract.Emails.MessageData.SolarCashOutData;
using SolarCoinAddressData = Lykke.Job.Messages.Contract.Emails.MessageData.SolarCoinAddressData;
using SwapRefundData = Lykke.Job.Messages.Contract.Emails.MessageData.SwapRefundData;
using SwiftCashoutDeclinedData = Lykke.Job.Messages.Contract.Emails.MessageData.SwiftCashoutDeclinedData;
using SwiftCashoutProcessedData = Lykke.Job.Messages.Contract.Emails.MessageData.SwiftCashoutProcessedData;
using SwiftCashOutRequestData = Lykke.Job.Messages.Contract.Emails.MessageData.SwiftCashOutRequestData;
using SwiftConfirmedData = Lykke.Job.Messages.Contract.Emails.MessageData.SwiftConfirmedData;
using TransferCompletedData = Lykke.Job.Messages.Contract.Emails.MessageData.TransferCompletedData;

namespace Lykke.Job.Messages.Core.Services.Email
{
    public interface IEmailGeneratorNew
    {
        Task<EmailMessage> GenerateLykkeCardVisaMsg(string partnerId, LykkeCardVisaData lykkeCardVisaData);
        Task<EmailMessage> GenerateWelcomeMsg(string partnerId, RegistrationMessageData kycOkData);
        Task<EmailMessage> GenerateWelcomeFxMsg(string partnerId, KycOkData kycOkData);
        Task<EmailMessage> GenerateConfirmEmailMsg(string partnerId, EmailComfirmationData registrationData);      
        Task<EmailMessage> GenerateNoRefundDepositDoneMsg(string partnerId, NoRefundDepositDoneData messageData);
        Task<EmailMessage> GenerateNoRefundOCashOutMsg(string partnerId, NoRefundOCashOutData messageData);
        Task<EmailMessage> GenerateBankCashInMsg(string partnerId, BankCashInData messageData);
        Task<EmailMessage> GenerateCashInRefundMsg(string partnerId, CashInRefundData messageData);
        Task<EmailMessage> GenerateUserRegisteredMsg(string partnerId, IPersonalData messageData);
        Task<EmailMessage> GenerateRejectedEmailMsg(string partnerId);
        EmailMessage GenerateFailedTransactionMsg(string partnerId, string transactionId, string[] clientIds);
        Task<EmailMessage> GenerateSwapRefundMsg(string partnerId, SwapRefundData messageData);
        Task<EmailMessage> GenerateOrdinaryCashOutRefundMsg(string partnerId, OrdinaryCashOutRefundData messageData);
        Task<EmailMessage> GenerateTransferCompletedMsg(string partnerId, TransferCompletedData transferCompletedData);
        Task<EmailMessage> GenerateDirectTransferCompletedMsg(string partnerId, DirectTransferCompletedData transferCompletedData);
        Task<EmailMessage> GenerateMyLykkeCashInMsg(string partnerId, MyLykkeCashInData messageData);
        Task<EmailMessage> GenerateRemindPasswordMsg(string partnerId, RemindPasswordData messageData);
        Task<EmailMessage> GeneratPrivateWalletAddressMsg(string partnerId, PrivateWalletAddressData messageData);
        Task<EmailMessage> GeneratSolarCashOutMsg(string partnerId, SolarCashOutData messageData);
        Task<EmailMessage> GeneratSolarAddressMsg(string partnerId, SolarCoinAddressData messageData);
        Task<EmailMessage> GenerateDeclinedDocumentsMsg(string partnerId, DeclinedDocumentsData messageData);
        Task<EmailMessage> GenerateCashoutUnlockMsg(string partnerId, CashoutUnlockData messageData);
        Task<EmailMessage> GenerateSwiftConfirmedMsg(string partnerId, SwiftConfirmedData messageData);
        Task<EmailMessage> GenerateSwiftCashoutRequestedMsg(string partnerId, SwiftCashoutRequestedData messageData);
        Task<EmailMessage> GenerateSwiftCashOutRequestMsg(string partnerId, SwiftCashOutRequestData messageData);
        Task<EmailMessage> GenerateRequestForDocumentMsg(string partnerId, RequestForDocumentData messageData);
        Task<EmailMessage> GenerateSwiftCashoutProcessedMsg(string partnerId, SwiftCashoutProcessedData messageData);
        Task<EmailMessage> GenerateSwiftCashoutDeclinedMsg(string partnerId, SwiftCashoutDeclinedData messageData);
        Task<EmailMessage> GenerateRegistrationVerifyEmailMsgAsync(string partnerId, RegistrationEmailVerifyData messageData);
    }
}
