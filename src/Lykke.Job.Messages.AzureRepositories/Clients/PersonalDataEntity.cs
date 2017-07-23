using System;
using Lykke.Job.Messages.Core.Domain.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.Messages.AzureRepositories.Clients
{
    public class PersonalDataEntity : TableEntity, IFullPersonalData
    {
        public static string GeneratePartitionKey()
        {
            return "PD";
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }


        public DateTime Regitered { get; set; }
        public string Id => RowKey;
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }
        public string CountryFromPOA { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string ReferralCode { get; set; }

        public string PasswordHint { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string CountryFromID { get; set; }

        public string SpotRegulator { get; set; }

        public string MarginRegulator { get; set; }
        public string PaymentSystem { get; set; }

        internal void Update(IPersonalData src)
        {
            Country = src.Country;
            Zip = src.Zip;
            City = src.City;
            Address = src.Address;
            ContactPhone = src.ContactPhone;
            FullName = src.FullName;
            FirstName = src.FirstName;
            LastName = src.LastName;
            SpotRegulator = src.SpotRegulator;
        }

        public static PersonalDataEntity Create(IPersonalData src)
        {
            var result = new PersonalDataEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.Id),
                Email = src.Email,
                Regitered = src.Regitered
            };

            result.Update(src);

            return result;
        }

        public static PersonalDataEntity Create(IFullPersonalData src)
        {
            var result = Create((IPersonalData)src);

            result.PasswordHint = src.PasswordHint;

            return result;
        }
    }
}