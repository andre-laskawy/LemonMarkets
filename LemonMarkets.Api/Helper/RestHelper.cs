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
                        var r = await httpClient.PostAsync(url, formContent).ConfigureAwait(false);
                        r.EnsureSuccessStatusCode();
                        return await r.Content.ReadAsStringAsync();
                    }
                }

                if (method == "GET")
                {
                    var response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }

                if (method == "PUT")
                {
                    using (HttpContent formContent = new FormUrlEncodedContent(payload))
                    {
                        var r = await httpClient.PutAsync(url, formContent).ConfigureAwait(false);
                        r.EnsureSuccessStatusCode();
                        return await r.Content.ReadAsStringAsync();
                    }
                }

                if (method == "DELETE")
                {
                    var response = await httpClient.DeleteAsync(url);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }

                throw new Exception("MakeRequest: Undefined METHOD");
            }
            catch
            {
                throw;
            }
        }
    }
}
