using System;

namespace Lykke.Job.Messages.Core.Domain.Clients
{
    public interface IPersonalData
    {
        DateTime Regitered { get; }
        string Id { get; }
        string Email { get; }
        string FullName { get; }
        string FirstName { get; set; }
        string LastName { get; set; }
        DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// ISO Alpha 2 code
        /// </summary>
        string CountryFromID { get; set; }


        /// <summary>
        /// ISO Alpha 3 code
        /// </summary>
        string Country { get; set; }

        /// <summary>
        /// ISO Alpha 3 code. Country from Proof of Address.
        /// </summary>
        string CountryFromPOA { get; set; }

        string Zip { get; set; }
        string City { get; }
        string Address { get; }
        string ContactPhone { get; }
        string ReferralCode { get; }
        string SpotRegulator { get; }
        string MarginRegulator { get; }
        string PaymentSystem { get; }
    }
}