using Lykke.Job.Messages.Core.Domain.DepositRefId;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Services.Deposits
{
    public class DepositRefIdGenService
    {
        public static async Task<string> GenerateReferenceId(string assetId, double amount, string email, string clientId, IDepositRefIdInUseRepository depositRefIdInUseRepository)
        {
            string date = DateTime.Now.ToString("ddMMMyyyy", CultureInfo.InvariantCulture);

            var usedCodes = await depositRefIdInUseRepository.GetAllUsedCodesAsync(clientId, date);
            string[] existingCodes = usedCodes.Select(_ => _.Code).ToArray();

            email = email.Replace("@", "..");
            Random r = new Random();
            string newCode = GenerateRandomCode(r, 'A', 'Z', existingCodes);
            if (newCode != null)
            {
                string refCode = $"{email}_{date}_{newCode}";
                depositRefIdInUseRepository.AddUsedCodesAsync(clientId, date, newCode, assetId, amount);
                return refCode;
            }
            return null;
        }

        private static string GenerateRandomCode(Random r, int min, int max, string[] existingCodes, bool nestedCall = false)
        {
            string newCode = null;

            // straightforward approach - try to generate a pure random code 
            for (int i = 0; i < 100; i++)
            {
                char ch1 = (Char)r.Next(min, max + 1);
                char ch2 = (Char)r.Next(min, max + 1);
                if (ch1 > 'Z')
                {
                    ch1 = (char)((int)'0' - 1 + (ch1 - 'Z'));
                }
                if (ch2 > 'Z')
                {
                    ch2 = (char)((int)'0' - 1 + (ch2 - 'Z'));
                }
                newCode = $"{ch1}{ch2}";
                if (!existingCodes.Contains(newCode))
                {
                    return newCode;
                }
            }

            // random code generation failed - obviously too many codes are exist
            // try to get the first free code
            if (newCode == null)
            {
                for (char ch1 = (char)min; ch1 <= max + 1; ch1++)
                {
                    for (char ch2 = (char)min; ch2 <= max + 1; ch2++)
                    {
                        if (ch1 > 'Z')
                        {
                            ch1 = (char)((int)'0' - 1 + (ch1 - 'Z'));
                        }
                        if (ch2 > 'Z')
                        {
                            ch2 = (char)((int)'0' - 1 + (ch2 - 'Z'));
                        }
                        newCode = $"{ch1}{ch2}";
                        if (!existingCodes.Contains(newCode))
                        {
                            return newCode;
                        }
                    }
                }
            }

            // no free codes remain
            // try to generate code including digits
            if (!nestedCall)
            {
                newCode = GenerateRandomCode(r, min, max + 10, existingCodes, true);
            }
            return null;
        }

    }
}
