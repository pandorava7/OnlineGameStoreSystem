using System.Net;
using System.Net.Mail;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string message);
}

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        string smtpServer = emailSettings["SmtpServer"];
        int smtpPort = int.Parse(emailSettings["SmtpPort"]);
        string senderEmail = emailSettings["SenderEmail"];
        string password = emailSettings["Password"];
        string senderName = emailSettings["SenderName"];

        var mail = new MailMessage();
        mail.From = new MailAddress(senderEmail, senderName);
        mail.To.Add(email);
        mail.Subject = subject;
        mail.Body = message;
        mail.IsBodyHtml = true;

        using (var smtp = new SmtpClient(smtpServer, smtpPort))
        {
            smtp.Credentials = new NetworkCredential(senderEmail, password);
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(mail);
        }
    }
}

