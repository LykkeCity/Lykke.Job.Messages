namespace Lykke.Job.Messages.Core.Extensions
{
    public static class StringExtensions
    {
        public static string HtmlBreaks(this string src)
        {
            return src.Replace("\r\n", "<br>");
        }
    }
}