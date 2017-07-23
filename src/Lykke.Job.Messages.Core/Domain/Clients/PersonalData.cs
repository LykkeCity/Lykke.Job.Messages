using System;

namespace Lykke.Job.Messages.Core.Domain.Clients
{
    public class PersonalData : IPersonalData
    {
        public DateTime Regitered { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// ISO Alpha 2 code
        /// </summary>
        public string CountryFromID { get; set; }
        /// <summary>
        /// ISO Alpha 3 code
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// ISO Alpha 3 code
        /// </summary>
        public string CountryFromPOA { get; set; }

        public string Zip { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string ReferralCode { get; set; }
        public string SpotRegulator { get; set; }
        public string MarginRegulator { get; set; }
        public string PaymentSystem { get; set; }
    }
}