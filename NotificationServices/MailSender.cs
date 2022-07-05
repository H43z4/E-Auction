using MimeKit;
using Models.Domain.Mail;
using System;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace NotificationServices
{
    public class EMailSender : IEMailSender
    {
        public Task SendUsingGmail(EmailSetting emailSetting)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(emailSetting.SenderName, emailSetting.Sender));
                message.To.Add(new MailboxAddress(emailSetting.SenderName, emailSetting.Receiver));
                message.Subject = emailSetting.Subject;
                message.Body = new TextPart("plain")
                {
                    Text = emailSetting.Body
                };

                using (var client = new SmtpClient())
                {
                    //client.Connect(emailSetting.Host, emailSetting.Port, false);

                    ////SMTP server authentication if needed
                    //client.Authenticate(emailSetting.Sender, emailSetting.Password);

                    //client.Send(message);

                    //client.Disconnect(true);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(false);
            }
        }

        public Task SendEmailUsingPunjabGovPk(EmailSetting emailSetting)
        {
            MailMessage message = new MailMessage();

            //var message = new MimeMessage();
            message.From = new MailAddress(emailSetting.Sender, emailSetting.SenderName);
            message.To.Add(emailSetting.Receiver);
            message.Subject = emailSetting.Subject;
            message.Body = emailSetting.Body;

            using (var client = new SmtpClient())
            {
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = true;
                client.EnableSsl = false;
                client.Host = emailSetting.Host;
                client.Port = emailSetting.Port;
                client.Credentials = new NetworkCredential(emailSetting.Sender, emailSetting.Password);
                client.Send(message);
            }

            return Task.FromResult(true);
        }
    }
}
