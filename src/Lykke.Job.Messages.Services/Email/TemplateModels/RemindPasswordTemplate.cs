namespace Lykke.Job.Messages.Services.Email.TemplateModels
{
    public class RemindPasswordTemplate
    {
        public RemindPasswordTemplate(string hint, int year)
        {
            Hint = hint;
            Year = year;
        }

        public string Hint { get; set; }
        public int Year { get; set; }
    }
}
