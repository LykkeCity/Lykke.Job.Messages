﻿using Lykke.Job.Messages.Contract.Emails.MessageData;
using Lykke.Service.EmailSender;
using Lykke.Service.PersonalData.Contract.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
