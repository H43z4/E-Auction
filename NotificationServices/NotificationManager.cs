using Models.Domain.Mail;
using Models.enums;
using SmsService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationServices
{
    public class NotificationManager : INotificationManager
    {
        readonly ISmsSender smsSender;
        readonly IEMailSender emailSender;

        public List<NotificationType> NotificationSentOn { get; private set; }

        public NotificationManager()
        {
            this.smsSender = new SmsSender();
            this.emailSender = new EMailSender();

            this.NotificationSentOn = new List<NotificationType>();
        }

        public async Task<bool> SendMessage(string phoneNumber, EmailSetting emailSetting)
        {
            this.NotificationSentOn.Clear();

            var smsTask = Task.Factory.StartNew(() => this.SendSMS(phoneNumber, emailSetting.Body));
            var emailTask = Task.Factory.StartNew(() => this.SendEmail(emailSetting));

            var tasks = new Task[] { smsTask, emailTask };

            await Task.WhenAll(tasks);

            return true;
        }

        public async Task<bool> SendAccountConfirmationMessages_1(string phoneNumber, EmailSetting emailSetting)
        {
            this.NotificationSentOn.Clear();

            await this.SendSMS(phoneNumber, emailSetting.Body);
            await this.SendEmail(emailSetting);

            //var tasks = new Task[] { smsTask, emailTask };

            //await Task.WhenAll(tasks);

            return true;
        }

        public async Task<bool> SendSMS(string phoneNumber, string msg)
        {
            try
            {
                await this.smsSender.SendSms(phoneNumber, msg);

                this.NotificationSentOn.Add(NotificationType.SMS);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendEmail(EmailSetting emailSetting)
        {
            try
            {
                await emailSender.SendEmailUsingPunjabGovPk(emailSetting);

                this.NotificationSentOn.Add(NotificationType.EMAIL);

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}