namespace LemonMarkets
{
    using LemonMarkets.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
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
        private static string apiDataBaseUrl = "https://paper-data.lemon.markets/v1/";
        private static string apiTradingBaseUrl = "https://paper-trading.lemon.markets/rest/v1/";

        public LemonApi()
        { }

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

        public async Task<LemonResult<Space>> GetSpaces()
        {
            try
            {
                var requestUrl = apiTradingBaseUrl + "spaces";
                var json = await MakeRequest(requestUrl, null, "GET");
                var results = JsonConvert.DeserializeObject<LemonResult<Space>>(json);
                return results;
            }
            catch
            {
                if (throwErrors) throw;
                return null;
            }
        }

        public async Task<Space> GetSpace(string uuid)
        {
            try
            {
                var requestUrl = apiTradingBaseUrl + "spaces/" + uuid + "/";
                var json = await MakeRequest(requestUrl, null, "GET");
                var result = JsonConvert.DeserializeObject<Space>(json);
                return result;
            }
            catch
            {
                if (throwErrors) throw;
                return null;
            }
        }


        public async Task<LemonResult<Order>> GetOrders(OrderSearchFilter filter)
        {
            try
            {
                if (filter == null || string.IsNullOrEmpty(filter.SpaceUuid))
                    throw new Exception("Space Uuid is required");

                var requestUrl = apiTradingBaseUrl + "spaces/" + filter.SpaceUuid + "/orders/";
                //todo: apply filter

                var json = await MakeRequest(requestUrl, null, "GET");
                var result = JsonConvert.DeserializeObject<LemonResult<Order>>(json);
                return result;
            }
            catch
            {
                if (throwErrors) throw;
                return null;
            }
        }

        public async Task<Space> GetOrder(string spaceUuid, string orderUuid)
        {
            return null;
        }


        public async Task<LemonResult<Instrument>> SearchWithFilter(InstrumentSearchFilter filter)
        {
            try
            {
                var resultSet = new LemonResult<Instrument>(){Results = new List<Instrument>()};

                var requestUrl = apiDataBaseUrl + "instruments?";
                
                var qryStr = new StringBuilder();
                if (filter.SearchByIsins == null || !filter.SearchByIsins.Any())
                    qryStr.Append("search=" + (string.IsNullOrEmpty(filter.SearchByString) ? "" : HttpUtility.UrlEncode(filter.SearchByString)));
                else
                    qryStr.Append("isin=" + string.Join(",", filter.SearchByIsins));

                if (filter.TradingVenue.HasValue)
                    qryStr.Append("&mic=" + filter.TradingVenue.GetValueOrDefault());
                    
                if (filter.Currency.HasValue)
                    qryStr.Append("&currency=" + filter.Currency);
                if (filter.InstrumentType.HasValue)
                    qryStr.Append("&type=" + filter.InstrumentType);
                if (filter.IsTradable.HasValue)
                    qryStr.Append("&tradable=" + (filter.IsTradable.GetValueOrDefault() ? "true" : "false"));

                var hasNextPage = true;
                var reqUrl = requestUrl + qryStr;
                while (hasNextPage)
                {
                    var json = await MakeRequest(reqUrl, null, "GET");
                    var results = JsonConvert.DeserializeObject<LemonResult<Instrument>>(json);
                    resultSet.Next = results.Next;
                    resultSet.Previous = results.Previous;

                    resultSet.Results.AddRange(results.Results);
                    hasNextPage = filter.WithPaging && !string.IsNullOrEmpty(results.Next);
                    if (hasNextPage)
                        reqUrl = resultSet.Next;
                }
                return resultSet;
            }
            catch
            {
                if (throwErrors) throw;
                return null;
            }
        }

        //deprecated
        public async Task<LemonResult<Instrument>> Search(string searchText, InstrumentType? type = null, string currency = null)
        {
            try
            {
                var reqUrl = apiDataBaseUrl + "instruments/";
                var paramUrl = $"?search={HttpUtility.UrlEncode(searchText)}";
                paramUrl = type == null ? paramUrl : paramUrl + $"&type={type}";
                paramUrl = currency == null ? paramUrl : paramUrl + $"&currency={currency.ToUpper()}";

                var json = await MakeRequest(reqUrl + paramUrl, null, "GET");
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
                var url = $"{apiDataBaseUrl}ohlc/d1?isin={symbol}";
                var json = await MakeRequest(url, null, "GET");
                
                var response = JsonConvert.DeserializeObject<LemonResult<ChartValue>>(json);
                return response.Results.FirstOrDefault();
            }
            catch
            {
                if (throwErrors) throw;
                return new ChartValue();
            }
        }

        public async Task<(double Ask, double Bid)> GetTicker(string isin)
        {
            try
            {
                string url = $"{apiDataBaseUrl}quotes?isin={isin}";
                var json = await MakeRequest(url, null, "GET");
                var jObject = JObject.Parse(json);
                var data = jObject.Get("results") as JArray;

                double bid = 0;
                double ask = 0;
                var first = data.FirstOrDefault() as JObject;
                if (first != null)
                {
                    bid = first.GetValue<double>("b");
                    ask = first.GetValue<double>("a");
                }

                // if no value is retured use the latest market close value
                if (bid == 0)
                {
                    url = $"{apiDataBaseUrl}ohlc/m1?isin={isin}";
                    json = await MakeRequest(url, null, "GET");
                    jObject = JObject.Parse(json);
                    data = jObject.Get("results") as JArray;
                    first = data.First as JObject;

                    return (first.GetValue<double>("c"), first.GetValue<double>("c"));
                }

                return (ask, bid);
            }
            catch
            {
                if (throwErrors) throw;
                return (0, 0);
            }
        }

        public async Task<List<ChartValue>> GetChart(string isin, DateTime from, DateTime? to = null)
        {
            var result = new List<ChartValue>();
            try
            {
                to = to ?? DateTime.UtcNow;
                var defaultDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                var currentDT = from;
                          
                long unixTime = ((DateTimeOffset)from).ToUnixTimeMilliseconds();
                string url = $"{apiDataBaseUrl}ohlc/m1/?isin={isin}&from={unixTime}&ordering=date";

                while(currentDT <= to)
                {
                    try
                    {
                        var json = await MakeRequest(url, null, "GET");
                        var response = JsonConvert.DeserializeObject<LemonResult<ChartValue>>(json);

                        foreach (var c in response.Results)
                        {
                            currentDT = DateTime.Parse(c.Timestamp).ToUniversalTime();
                            if (currentDT <= to)
                            {
                                c.Created = currentDT;
                                c.Symbol = isin;
                                result.Add(c);
                            }
                        }

                        if (url.Contains("to="))
                        {
                            var prevUrl = response.Previous;
                            var dateUntilIdx = prevUrl.IndexOf("to=");
                            var prevDateUnix = prevUrl.Substring(dateUntilIdx + 3).Replace(".0", string.Empty);
                            var datePrev = defaultDate.AddMilliseconds(long.Parse(prevDateUnix));

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
                    expireDate = expireDate.AddSeconds(expire - 60); // subscract a little delay to ensure a smooth token refresh
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
