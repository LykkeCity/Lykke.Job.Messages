namespace Lykke.Job.Messages.Services.Sms.Nexmo
{
    public enum NexmoStatusCode
    {
        Success = 0,
        Throttled = 1,
        MissingParams = 2,
        InvalidParams = 3,
        InvalidCredentials = 4,
        InternalError = 5,
        InvalidMessage = 6,
        NumberBarred = 7,
        PartnerAccountBarred = 8,
        PartnerQuotaExceeded = 9,
        AccountNotEnabledForRest = 11,
        MessageTooLong = 12,
        CommunicationFailed = 13,
        InvalidSignature = 14,
        InvalidSenderAddress = 15,
        InvalidTTL = 16,
        FacilityNotAllowed = 19,
        InvalidMessageClass = 20,
        BadCallback = 23,
        NonWhiteListedDestination = 29
    }
}