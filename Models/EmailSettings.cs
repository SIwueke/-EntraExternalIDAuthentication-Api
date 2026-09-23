namespace ExternalIdDemo.Api.Models
{
    public class EmailSettings
    {
        public string MailServer { get; set; } = string.Empty;
        public int MailServerPort { get; set; } 
        public string MailFrom { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
