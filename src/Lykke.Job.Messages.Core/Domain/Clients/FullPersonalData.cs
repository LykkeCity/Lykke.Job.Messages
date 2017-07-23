using System;

namespace Lykke.Job.Messages.Core.Domain.Clients
{
    public class FullPersonalData : IFullPersonalData
    {
        public DateTime Regitered { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string CountryFromID { get; set; }
        public string Country { get; set; }
        public string CountryFromPOA { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string ReferralCode { get; set; }
        public string PasswordHint { get; set; }
        public string SpotRegulator { get; set; }
        public string MarginRegulator { get; set; }
        public string PaymentSystem { get; set; }
    }
}