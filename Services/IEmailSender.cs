using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;


namespace hmyapi.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message,string jsonToEncode);
    }
    public class EmailSender : IEmailSender
    {
        private SmtpClient Cliente { get; }
        private EmailSenderOptions Options { get; }

        public EmailSender(IOptions<EmailSenderOptions> options)
        {
            Options = options.Value;
            Cliente = new SmtpClient()
            {
                Host = Options.Host,
                Port = Options.Port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Options.Email, Options.Password),
                EnableSsl = Options.EnableSsl,
            };
        }

        public Task SendEmailAsync(string email, string subject, string message,string jsonToEncode)
        {
            var correo = new MailMessage(from: Options.Email, to: email, subject: subject, body: message);
            correo.Attachments.Add(new Attachment(new MemoryStream(Encoding.UTF8.GetBytes(jsonToEncode)), "HMY Color Configurator.json"));
            correo.IsBodyHtml = true;
            return Cliente.SendMailAsync(correo);
        }
    }
}
