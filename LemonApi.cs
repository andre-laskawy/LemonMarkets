using System.Dynamic;
using System.Globalization;
using LemonMarkets.Extensions;
using LemonMarkets.Models.Enums;

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
        private static bool throwErrors;

        private static string token;

        private static string apiDataBaseUrl = "https://data.lemon.markets/v1/";
        
        private static string apiTradingBaseUrl = "https://paper-trading.lemon.markets/rest/v1/";

        public LemonApi()
        { }

        public static void Init(string bearerToken, bool throwExceptions = true)
        {
            try
            {
                token = bearerToken;
                throwErrors = throwExceptions;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                    SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Ssl2 | SslProtocols.Ssl3
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<PostedOrder> PostOrder(PostOrderQuery query)
        {
            try
            {
                var requestUrl = apiTradingBaseUrl + "spaces/" + query.SpaceUuid + "/orders/";
                
                if (query.ValidUntil < DateTime.UtcNow)
                    throw new Exception("Can't post order: Valid Until < now");
                if(query.Side == OrderSide.All)
                    throw new Exception("Can't post order: OrderSide not specified");

                var qryParams = new Dictionary<string, string>
                {
                    {"isin", query.Isin},
                    {"valid_until", query.ValidUntil.ToUnixDt().ToString()},
                    {"side", query.Side.ToString().ToLower()},
                    {"quantity", query.Quantity.ToString()}
                };

                if(query.StopPrice.HasValue)
                    qryParams.Add("stop_price", query.StopPrice.Value.ToString(CultureInfo.InvariantCulture));
                if (query.LimitPrice.HasValue)
                    qryParams.Add("limit_price", query.LimitPrice.Value.ToString(CultureInfo.InvariantCulture));

                var json = await MakeRequest(requestUrl, qryParams);
                var result = JsonConvert.DeserializeObject<PostedOrder>(json);
                return result;
            }
            catch
            {
                if (throwErrors) throw;
                return null;
            }
        }

        public async Task<bool> DeleteOrder(string spaceUuid, string orderUuid)
        {
            try
            {
                var requestUrl = apiTradingBaseUrl + "spaces/" + spaceUuid + "/orders/" + orderUuid + "/";
                var json = await MakeRequest(requestUrl, null, "DELETE");
                return true;
            }
            catch
            {
                if (throwErrors) throw;
                return false;
            }
        }

        public async Task<bool> ActivateOrder(string spaceUuid, string orderUuid)
        {
            try
            {
                var requestUrl = apiTradingBaseUrl + "spaces/" + spaceUuid + "/orders/" + orderUuid + "/activate";
                var json = await MakeRequest(requestUrl, null, "PUT");
                return true;
            }
            catch
            {
                if (throwErrors) throw;
                return false;
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

                var requestUrl = apiTradingBaseUrl + "spaces/" + filter.SpaceUuid + "/orders";

                var parameters = new List<string>();

                if(filter.From.HasValue)
                    parameters.Add("created_at_from=" + filter.From.ToUnixDt());
                if (filter.To.HasValue)
                    parameters.Add("created_at_until=" + filter.To.ToUnixDt());
                if(filter.Side != OrderSide.All)
                    parameters.Add("side=" + filter.Side.ToString().ToLower());
                if (filter.Type != OrderType.All)
                    parameters.Add("type=" + filter.Type.ToString().ToLower());
                
                if (parameters.Any())
                    requestUrl += "?" + string.Join("&", parameters);

                var result = new LemonResult<Order>();

                var hasNextPage = true;
                while (hasNextPage)
                {
                    var json = await MakeRequest(requestUrl, null, "GET");
                    var res = JsonConvert.DeserializeObject<LemonResult<Order>>(json);
                    hasNextPage = filter.WithPaging && !string.IsNullOrEmpty(res.Next);
                    if (!hasNextPage)
                        return res;
                    requestUrl = res.Next;
                    result.Results.AddRange(res.Results);
                    result.Next = res.Next;
                    result.Previous = res.Previous;
                }
                return result;
            }
            catch
            {
                if (throwErrors) throw;
                return null;
            }
        }

        // TODO
        public async Task<Space> GetOrder(string spaceUuid, string orderUuid)
        {
            return await Task.FromResult<Space>(null);
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
                var utcTime = HttpUtility.UrlEncode(from.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
                string url = $"{apiDataBaseUrl}ohlc/m1/?mic=XMUN&isin={isin}&from={utcTime}&decimals=true&epoch=false&sorting=asc&limit=250";

                while (from <= to)
                {
                    try
                    {
                        var json = await MakeRequest(url, null, "GET");
                        var response = JsonConvert.DeserializeObject<LemonResult<ChartValue>>(json);

                        foreach (var c in response.Results)
                        {
                            from = DateTime.Parse(c.Timestamp).ToUniversalTime();
                            if (from <= to)
                            {
                                c.Created = from;
                                c.Symbol = isin;
                                result.Add(c);
                            }
                        }

                        if (response.Next is null
                            && from <= to)
                        {
                            from = from.AddHours(3);
                            utcTime = HttpUtility.UrlEncode(from.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
                            url = $"{apiDataBaseUrl}ohlc/m1/?mic=XMUN&isin={isin}&from={utcTime}&decimals=true&epoch=false&sorting=asc&limit=250";
                        }
                        else
                        {
                            url = response.Next;
                        }
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

        public async Task<LemonResult<Instrument>> Search(InstrumentSearchFilter filter)
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

                /*if (filter.TradingVenue.HasValue)
                    qryStr.Append("&mic=" + filter.TradingVenue.GetValueOrDefault());*/
                    
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

        private async Task<string> MakeRequest(string url, Dictionary<string, string> payload = null, string method = "POST")
        {
            try
            {
                var httpClient = HttpClientFactory.Create();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

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
                    if (payload == null)
                        payload = new Dictionary<string, string>();

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
