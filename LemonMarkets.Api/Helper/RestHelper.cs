using LemonMarkets.Models;
using LemonMarkets.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LemonMarkets.Helper
{
    public static class RestHelper
    {
        static string token;

        public static void Init(string bearerToken)
        {
            token = bearerToken;
        }

        public static async Task<string> MakeRequest(
            this string url, 
            Dictionary<string, string> payload = null, 
            string method = "POST")
        {
            HttpResponseMessage responseMessage = null;
            try
            {
                var httpClient = HttpClientFactory.Create();
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                payload = payload ?? new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                }
                    
                if (method == "POST")
                {
                    using (HttpContent formContent = new FormUrlEncodedContent(payload))
                    {
                        responseMessage = await httpClient.PostAsync(url, formContent).ConfigureAwait(false);
                        responseMessage.EnsureSuccessStatusCode();
                        return await responseMessage.Content.ReadAsStringAsync();
                    }
                }

                if (method == "GET")
                {
                    responseMessage = await httpClient.GetAsync(url);
                    responseMessage.EnsureSuccessStatusCode();
                    return await responseMessage.Content.ReadAsStringAsync();
                }

                if (method == "PUT")
                {
                    using (HttpContent formContent = new FormUrlEncodedContent(payload))
                    {
                        responseMessage = await httpClient.PutAsync(url, formContent).ConfigureAwait(false);
                        responseMessage.EnsureSuccessStatusCode();
                        return await responseMessage.Content.ReadAsStringAsync();
                    }
                }

                if (method == "DELETE")
                {
                    responseMessage = await httpClient.DeleteAsync(url);
                    responseMessage.EnsureSuccessStatusCode();
                    return await responseMessage.Content.ReadAsStringAsync();
                }

                throw new Exception("MakeRequest: Undefined METHOD");
            }
            catch
            {
                throw new Exception($"HTTP Error: {responseMessage.StatusCode}: {await responseMessage.Content.ReadAsStringAsync()}");
            }
        }
    }
}
