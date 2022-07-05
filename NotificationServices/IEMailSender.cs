//using Microsoft.AspNetCore.Identity.UI.Services;
using Models.Domain.Mail;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NotificationServices
{
    public interface IEMailSender
    {
        Task SendEmailUsingPunjabGovPk(EmailSetting emailSetting);
        Task SendUsingGmail(EmailSetting emailSetting);
    }
}