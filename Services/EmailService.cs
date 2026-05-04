using System.Net;
using System.Net.Mail;


namespace cake_shop.Services
{
    public class EmailService
    {
        public void SendEmail(string to, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("your@email.com", "App password"),
                EnableSsl = true
            };

            smtp.Send("your@emailc.com", to, subject, body);
        }
    }
}
