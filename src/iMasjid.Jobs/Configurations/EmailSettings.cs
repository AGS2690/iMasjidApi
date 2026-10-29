namespace iMasjid.Jobs.Configurations;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpServer { get; set; } = "smtp.example.com";
    public int Port { get; set; } = 587;
    public string SenderName { get; set; } = "iMasjid Security";
    public string SenderEmail { get; set; } = "noreply@imasjid.com";
    public string Username { get; set; } = "your_smtp_username";
    public string Password { get; set; } = "your_smtp_password";
    public bool EnableSsl { get; set; } = true;
    public bool Enabled { get; set; } = false;
}
