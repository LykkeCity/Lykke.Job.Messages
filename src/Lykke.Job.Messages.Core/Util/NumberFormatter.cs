using System.Globalization;
using Common;

namespace Lykke.Job.Messages.Core.Util
{
    public static class NumberFormatter
    {
        public static string FormatNumber(decimal number, int accuracy)
        {
            var formattedNumber = number.TruncateDecimalPlaces(accuracy).ToString(CultureInfo.InvariantCulture);

            return formattedNumber;
        }

        public static string FormatNumber(double number, int accuracy)
        {
            var formattedNumber = number.TruncateDecimalPlaces(accuracy).ToString(CultureInfo.InvariantCulture);

            return formattedNumber;
        }
    }
}
