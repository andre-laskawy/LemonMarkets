namespace LemonMarkets
{
    using LemonMarkets.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Authentication;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// Defines the <see cref="LemonApi" />.
    /// </summary>
    public class LemonApi
    {
        private static bool throwErrors, useCaching;

        private static Semaphore semaphore = new Semaphore(1, 1);

        private static HttpClient httpClient = null;

        private static string clientId, clientSecret;

        private static DateTime tokenExpireDate;

        public static async Task Init(string lemonClientId, string lemonClientSecret, bool throwExceptions = true)
        {
            try
            {
                throwErrors = throwExceptions;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                    SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Ssl2 | SslProtocols.Ssl3
                };

                httpClient = new HttpClient(httpClientHandler);
                clientId = lemonClientId;
                clientSecret = lemonClientSecret;

                var tokenResult = await GetLemonToken();
                tokenExpireDate = tokenResult.expireDate;
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenResult.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<LemonResult<Instrument>> Search(string searchText, InstrumentType? type = null, string currency = null)
        {
            try
            {
                string baseUrl = $"https://paper.lemon.markets/rest/v1/instruments/";
                var paramUrl = $"?search={HttpUtility.UrlEncode(searchText)}";
                paramUrl = type == null ? paramUrl : paramUrl + $"&type={type}";
                paramUrl = currency == null ? paramUrl : paramUrl + $"&currency={currency.ToUpper()}";

                var json = await MakeRequest(baseUrl + paramUrl, null, "GET");
                return JsonConvert.DeserializeObject<LemonResult<Instrument>>(json);
            }
            catch
            {
                if (throwErrors) throw;
                return null;
            }
        }

        public async Task<ChartValue> GetDailyOHLC(string symbol)
        {
            try
            {
                string url = $"https://paper.lemon.markets/rest/v1/trading-venues/XMUN/instruments/{symbol}/data/ohlc/D1/latest/";
                var json = await MakeRequest(url, null, "GET");
                var value = JsonConvert.DeserializeObject<ChartValue>(json);
                return value;
            }
            catch
            {
                if (throwErrors) throw;
                return new ChartValue();
            }
        }

        public async Task<(double Ask, double Bid)> GetTicker(string symbol)
        {
            try
            {
                string url = $"https://paper.lemon.markets/rest/v1/trading-venues/XMUN/instruments/{symbol}/data/quotes/latest/";
                var json = await MakeRequest(url, null, "GET");
                var jObject = JObject.Parse(json);
                var bid = jObject.GetValue<double>("b");
                var ask = jObject.GetValue<double>("a");

                // if no value is retured use the latest market close value
                if (bid == 0)
                {
                    url = $"https://paper.lemon.markets/rest/v1/trading-venues/XMUN/instruments/{symbol}/data/ohlc/m1/latest/";
                    json = await MakeRequest(url, null, "GET");

                    return (jObject.GetValue<double>("c"), jObject.GetValue<double>("c"));
                }

                return (ask, bid);
            }
            catch
            {
                if (throwErrors) throw;
                return (0, 0);
            }
        }

        public async Task<List<ChartValue>> GetChart(string symbol, DateTime from, DateTime? to = null)
        {
            var result = new List<ChartValue>();
            try
            {
                to = to ?? DateTime.UtcNow;
                var defaultDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                var currentDT = from;
                          
                long unixTime = ((DateTimeOffset)from).ToUnixTimeSeconds();
                string url = $"https://paper.lemon.markets/rest/v1/trading-venues/XMUN/instruments/{symbol}/data/ohlc/m1/?date_from={unixTime}&ordering=date";

                while(currentDT <= to)
                {
                    try
                    {
                        var json = await MakeRequest(url, null, "GET");
                        var response = JsonConvert.DeserializeObject<LemonResult<ChartValue>>(json);

                        foreach (var c in response.Results)
                        {
                            currentDT = defaultDate.AddSeconds(c.Timestamp);
                            if (currentDT <= to)
                            {
                                c.Created = currentDT;
                                c.Symbol = symbol;
                                result.Add(c);
                            }
                        }

                        if (url.Contains("date_until"))
                        {
                            var prevUrl = response.Previous;
                            var dateUntilIdx = prevUrl.IndexOf("date_until=");
                            var prevDateUnix = prevUrl.Substring(dateUntilIdx + 11).Replace(".0", string.Empty);
                            var datePrev = defaultDate.AddSeconds(long.Parse(prevDateUnix));

                            if (datePrev > to)
                            {
                                break;
                            }
                        }

                        url = response.Next;
                    }
                    catch
                    {
                        break;
                    }
                }

            }
            catch
            {
                if (throwErrors) throw;
                return null;
            }

            return result.OrderBy(p => p.Created).ToList();
        }

        private async Task<string> MakeRequest(string url, Dictionary<string, string> payload = null, string method = "POST")
        {
            try
            {
                var client = await GetHttpClient();                
                if (method == "POST")
                {
                    using (HttpContent formContent = new FormUrlEncodedContent(payload))
                    {
                        var r = await client.PostAsync(url, formContent).ConfigureAwait(false);
                        r.EnsureSuccessStatusCode();
                        return await r.Content.ReadAsStringAsync();
                    }
                }
                else
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch
            {
                throw;
            }
        }

        private async Task<HttpClient> GetHttpClient()
        {
            if (tokenExpireDate < DateTime.UtcNow)
            {
                try
                {
                    semaphore.WaitOne();
                    var tokeResult = await GetLemonToken();
                    tokenExpireDate = tokeResult.expireDate;

                    if (httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    {
                        httpClient.DefaultRequestHeaders.Remove("Authorization");
                    }

                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokeResult.Token);
                }
                catch
                { }
                finally
                {
                    semaphore.Release();
                }
            }

            return httpClient;
        }

        private static async Task<(string Token, DateTime expireDate)> GetLemonToken()
        {
            var result = string.Empty;
            var expireDate = DateTime.UtcNow;
            try
            {
                var data = new Dictionary<string, string>();
                data.Add("client_id", clientId);
                data.Add("client_secret", clientSecret);
                data.Add("grant_type", "client_credentials");

                string url = "https://auth.lemon.markets/oauth2/token";
                var client = new HttpClient();

                using (HttpContent formContent = new FormUrlEncodedContent(data))
                {
                    var response = await client.PostAsync(url, formContent).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(json);
                    result = jObject.GetValue<string>("access_token");
                    var expire = jObject.GetValue<double>("expires_in");
                    expireDate.AddSeconds(expire - 60); // subscract a little delay to ensure a smooth token refresh
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return (result, expireDate);
        }
    }
}
