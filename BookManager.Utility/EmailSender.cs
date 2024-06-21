using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BookManager.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                // logic send email
                MailMessage mailMessage = new(_configuration["NetMail:sender"], email)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = htmlMessage
                };
                SmtpClient client = new SmtpClient
                {
                    Port = _configuration.GetValue<int>("NetMail:port"),
                    Host = _configuration["NetMail:smtpHost"],
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential()
                    {
                        UserName = _configuration["NetMail:sender"],
                        Password = _configuration["NetMail:senderpassword"]
                    }
                };

                return client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
