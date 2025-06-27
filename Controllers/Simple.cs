using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace t23p0.Controllers
{
    public class SimpleMailController : Controller
    {
        private readonly IConfiguration _configuration;

        public SimpleMailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult SendTestMail(string recipientEmail)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(username, password);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = "Test Email",
                        Body = "This is a simple test email from the system.",
                        IsBodyHtml = false
                    };

                    mailMessage.To.Add(recipientEmail);

                    client.Send(mailMessage);
                }

                return Json(new { success = true, message = "Test email sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error sending email: {ex.Message}" });
            }
        }
    }
}
