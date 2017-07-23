namespace Lykke.Job.Messages.Services.Email
{
    public static class MediaTypeNames
    {
        /// <summary>Specifies the type of text data in an e-mail message attachment.</summary>
        public static class Text
        {
            /// <summary>Specifies that the <see cref="T:System.Net.Mime.MediaTypeNames.Text" /> data is in plain text format.</summary>
            public const string Plain = "text/plain";

            /// <summary>Specifies that the <see cref="T:System.Net.Mime.MediaTypeNames.Text" /> data is in HTML format.</summary>
            public const string Html = "text/html";

            /// <summary>Specifies that the <see cref="T:System.Net.Mime.MediaTypeNames.Text" /> data is in XML format.</summary>
            public const string Xml = "text/xml";

            /// <summary>Specifies that the <see cref="T:System.Net.Mime.MediaTypeNames.Text" /> data is in Rich Text Format (RTF).</summary>
            public const string RichText = "text/richtext";
        }
    }
}