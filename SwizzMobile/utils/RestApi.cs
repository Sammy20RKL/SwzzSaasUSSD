using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SwizzMobile.Utils
{
    public class RestApi
    {
        public async Task<string> TestRequest(string sessionId, string phone_no, string body)
        {
            string apiResponse = string.Empty;
            string BaseUrl = "http://jngatia-001-site1.itempurl.com/whatsapp/bot/";
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(BaseUrl + string.Format("testussd?sessionNo={0}&phoneNo={1}&body={2}", sessionId, phone_no, body)))
                {
                    apiResponse = await response.Content.ReadAsStringAsync();
                }
            }
            return apiResponse;
        }
    }
}