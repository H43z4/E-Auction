using Models.Domain.Mail;
using Models.enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationServices
{
    public interface INotificationManager
    {
        List<NotificationType> NotificationSentOn { get; }

        Task<bool> SendEmail(EmailSetting emailSetting);
        Task<bool> SendMessage(string phoneNumber, EmailSetting emailSetting);
        Task<bool> SendSMS(string phoneNumber, string msg);
    }
}