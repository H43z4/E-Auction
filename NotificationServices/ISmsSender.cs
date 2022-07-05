using System.Threading.Tasks;

namespace SmsService
{
    public interface ISmsSender
    {
        Task<bool> SendSms(string phone, string msg);
    }
}