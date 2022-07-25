using Models.Domain.EPay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EPayment
{
    public class EPaymentService
    {
        private readonly List<ePayAPIs> ePayAPIs;
        private string EPaymentToken { get; set; }

        public EPaymentService(List<ePayAPIs> ePayAPIs)
        {
            this.ePayAPIs = ePayAPIs;
        }

        private async Task<bool> AuthenticateAsync()
        {
            try
            {
                var response = false;

                var api = this.ePayAPIs.FirstOrDefault(x => x.ApiName == "auth");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var json = JsonConvert.SerializeObject(new 
                    { 
                        clientId = api.ClientId, 
                        clientSecretKey = api.ClientSecretKey 
                    });

                    var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    dynamic dynObj = new { };

                    await httpClient.PostAsync(new Uri(api.RequestURL), stringContent)
                        .ContinueWith((task) =>
                        {
                            task.Result.EnsureSuccessStatusCode();

                            var content = task.Result.Content.ReadAsStringAsync().Result;

                            dynObj = JsonConvert.DeserializeObject<dynamic>(content);
                        });

                    if (dynObj.status == "OK")
                    {
                        this.EPaymentToken = Convert.ToString(dynObj.content[0].token.accessToken);

                        response = true;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<Models.Views.EPay.ePayApplication> GetPSIdAsync(Models.Views.EPay.ePayApplication ePayApplication)
        {
            try
            {
                var api = this.ePayAPIs.FirstOrDefault(x => x.ApiName == "psid");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.EPaymentToken);

                    var data = new 
                    {
                        deptTransactionId = ePayApplication.deptTransactionId,
                        ain = ePayApplication.ain,
                        consumerName = ePayApplication.consumerName,
                        mobileNo = ePayApplication.mobileNo,
                        cnic = ePayApplication.cnic,
                        ntn = ePayApplication.ntn,
                        companyName = ePayApplication.companyName,
                        emailAddress = ePayApplication.emailAddress,
                        series = ePayApplication.series,
                        registrationMark = ePayApplication.registrationMark,
                        chassisNo = ePayApplication.chassisNo,
                        ownerName = ePayApplication.ownerName,
                        dueDate = ePayApplication.dueDate.ToString("yyyy-MM-dd"),
                        amountWithinDueDate = ePayApplication.amountToTransfer,
                        amountBifurcation = new [] 
                        {
                            new
                            {
                                accountHeadName = ePayApplication.accountHeadName,
                                accountNumber = ePayApplication.accountNumber,
                                amountToTransfer = ePayApplication.amountToTransfer
                            }
                        }
                    };

                    var json = JsonConvert.SerializeObject(data);

                    var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    dynamic dynObj = new { };

                    await httpClient.PostAsync(new Uri(api.RequestURL), stringContent)
                        .ContinueWith((task) =>
                        {
                            task.Result.EnsureSuccessStatusCode();

                            var content = task.Result.Content.ReadAsStringAsync().Result;

                            dynObj = JsonConvert.DeserializeObject<dynamic>(content);
                        });

                    if (dynObj.status == "OK")
                    {
                        ePayApplication.psId = Convert.ToString(dynObj.content[0].consumerNumber);
                    }
                }

                return ePayApplication;
            }
            catch (Exception ex)
            {
                ePayApplication.psId = ex.Message;
                return ePayApplication;
            }
        }

        public async Task<List<Models.Views.EPay.ePayApplication>> GeneratePSIdAsync(List<Models.Views.EPay.ePayApplication> ePayApplications)
        {
            var authorized = await this.AuthenticateAsync();

            if (authorized)
            {
                foreach (var app in ePayApplications)
                {
                    await this.GetPSIdAsync(app)
                        .ContinueWith((ePayAppTask) =>
                        {
                            app.psId = ePayAppTask.Result.psId;
                        });
                }
            }

            return ePayApplications;
        }
    }
}
