using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Core.Util
{
    public static class NumberFormatter
    {
        public static string FormatNumber(decimal number, int accuracy)
        {
            string formattedNumber = number.ToString($"F{accuracy}").TrimEnd('0').TrimEnd(new char[] {',', '.'});

            return formattedNumber;
        }

        public static string FormatNumber(double number, int accuracy)
        {
            string formattedNumber = number.ToString($"F{accuracy}").TrimEnd('0').TrimEnd(new char[] { ',', '.' });

            return formattedNumber;
        }
    }
}
