using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SmsService
{
    public class SmsSender : ISmsSender
    {
        public async Task<bool> SendSms(string phone, string msg)
        {
            try
            {
                var response = false;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var multipartFormDataContent = new MultipartFormDataContent();

                    var data = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("sec_key", "8deb3d200c5ca512711d0157ae0490f8"),
                        new KeyValuePair<string, string>("sms_text", msg),
                        new KeyValuePair<string, string>("phone_no", phone),
                        new KeyValuePair<string, string>("sms_language", "english")
                    };

                    foreach (var keyValuePair in data)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value),
                            String.Format("\"{0}\"", keyValuePair.Key));
                    }

                    response = await httpClient.PostAsync(new Uri($"https://smsgateway.pitb.gov.pk/api/send_sms"), multipartFormDataContent)
                        .ContinueWith((task) =>
                        {
                            task.Result.EnsureSuccessStatusCode();

                            var content = task.Result.Content.ReadAsStringAsync().Result;
                            //return JsonConvert.DeserializeObject<bool>(content);

                            return content.Contains("success") ? true : false;
                        });
                }

                return response;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
