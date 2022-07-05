using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using Models.Domain.Mail;
using System;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace MailService
{
    public class MailSender : IEmailSender
    {
        public EmailSetting EmailSetting { get; set; }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return this.SendEmailUsingPunjabGovPk(email, subject, htmlMessage);
        }

        public Task SendUsingGmail(string email, string subject, string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(this.EmailSetting.SenderName, this.EmailSetting.Sender));
                message.To.Add(new MailboxAddress(this.EmailSetting.SenderName, this.EmailSetting.Receiver));
                message.Subject = this.EmailSetting.Subject;
                message.Body = new TextPart("plain")
                {
                    Text = this.EmailSetting.Body
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(this.EmailSetting.Host, this.EmailSetting.Port, false);

                    //SMTP server authentication if needed
                    client.Authenticate(this.EmailSetting.Sender, this.EmailSetting.Password);

                    client.Send(message);

                    client.Disconnect(true);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(false);
            }
        }

        public Task SendEmailUsingPunjabGovPk_0(string email, string subject, string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(this.EmailSetting.SenderName, this.EmailSetting.Sender));
                message.To.Add(new MailboxAddress(this.EmailSetting.SenderName, this.EmailSetting.Receiver));
                message.Subject = this.EmailSetting.Subject;
                message.Body = new TextPart("plain")
                {
                    Text = this.EmailSetting.Body
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(this.EmailSetting.Host, this.EmailSetting.Port, MailKit.Security.SecureSocketOptions.Auto);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    //SMTP server authentication if needed
                    client.Authenticate(this.EmailSetting.Sender, this.EmailSetting.Password);

                    client.Send(message);

                    client.Disconnect(true);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(false);
            }
        }

        public Task SendEmailUsingPunjabGovPk_1(string email, string subject, string htmlMessage)
        {
            try
            {
                MailMessage message = new MailMessage();

                //var message = new MimeMessage();
                message.From = new MailAddress(this.EmailSetting.Sender, this.EmailSetting.SenderName);
                message.To.Add(this.EmailSetting.Receiver);
                message.Subject = this.EmailSetting.Subject;
                message.Body = this.EmailSetting.Body;

                using (var client = new SmtpClient())
                {
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = true;
                    client.EnableSsl = false;
                    client.Host = this.EmailSetting.Host;
                    client.Port = this.EmailSetting.Port;
                    client.Credentials = new NetworkCredential(this.EmailSetting.Sender, this.EmailSetting.Password);
                    client.Send(message);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                return Task.FromResult(false);
            }
        }

        public Task SendEmailUsingPunjabGovPk(string email, string subject, string htmlMessage)
        {
            MailMessage message = new MailMessage();

            //var message = new MimeMessage();
            message.From = new MailAddress(this.EmailSetting.Sender, this.EmailSetting.SenderName);
            message.To.Add(this.EmailSetting.Receiver);
            message.Subject = this.EmailSetting.Subject;
            message.Body = this.EmailSetting.Body;

            using (var client = new SmtpClient())
            {
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = true;
                client.EnableSsl = false;
                client.Host = this.EmailSetting.Host;
                client.Port = this.EmailSetting.Port;
                client.Credentials = new NetworkCredential(this.EmailSetting.Sender, this.EmailSetting.Password);
                client.Send(message);
            }

            return Task.FromResult(true);
        }
    }
}
