using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HtmlScraper.Code
{
    public static class HttpClientProxy
    {
        public static async Task<HttpResponseMessage> Post(string baseApiUrl, string apiEndpointUrl, string data)
        {
            HttpResponseMessage response;
            using (HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(baseApiUrl) })
            {
                using (var httpContent = new StringContent(data, Encoding.UTF8)) //, "text/plain"))
                {
                    //httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    httpContent.Headers.ContentType.MediaType = "application/json";

                    response = await httpClient.PostAsync(apiEndpointUrl, httpContent);
                }
            }
            return response;
        }
    }
}
