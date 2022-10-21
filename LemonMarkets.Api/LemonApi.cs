using System.Dynamic;
using System.Globalization;
using LemonMarkets.Extensions;
using LemonMarkets.Models.Enums;

namespace LemonMarkets
{
    using IO.Ably;
    using IO.Ably.Realtime;
    using LemonMarkets.Helper;
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
        public static bool ThrowErrors { get; set; }

        public static string UserId { get; internal set; }

        public static bool Token { get; internal set; }

        internal static AblyRealtime StreamingClient { get; set; }

        internal static Dictionary<int, IRealtimeChannel> ChannelCollection = new Dictionary<int, IRealtimeChannel>();

        private static string apiDataBaseUrl = "https://data.lemon.markets/v1/";

        private static string apiPaperTradingBaseUrl = "https://paper-trading.lemon.markets/v1/";

        private static string apiTradingBaseUrl = "https://trading.lemon.markets/v1/";

        public LemonApi(bool usePaperTrading = false)
        {
            apiTradingBaseUrl = usePaperTrading ? apiPaperTradingBaseUrl : apiTradingBaseUrl;
        }

        // Orders

        public async Task<SingleLemonResult<PostedOrder>> PostOrder(PostOrderQuery query)
        {
            try
            {
                var requestUrl = apiTradingBaseUrl + "orders";

                if (query.ValidUntil < DateTime.UtcNow)
                    throw new Exception("Can't post order: Valid Until < now");
                if (query.Side == OrderSide.All)
                    throw new Exception("Can't post order: OrderSide not specified");

                var qryParams = new Dictionary<string, string>
                {
                    {"isin", query.Isin},
                    {"expires_at",  query.ValidUntil.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture) },
                    {"side", query.Side.ToString().ToLower()},
                    {"quantity", query.Quantity.ToString()}
                };

                if (query.StopPrice.HasValue)
                    qryParams.Add("stop_price", query.StopPrice.Value.ToString(CultureInfo.InvariantCulture));
                if (query.LimitPrice.HasValue)
                    qryParams.Add("limit_price", query.LimitPrice.Value.ToString(CultureInfo.InvariantCulture));

                var json = await requestUrl.MakeRequest(qryParams, "POST");
                var result = JsonConvert.DeserializeObject<SingleLemonResult<PostedOrder>>(json);
                return result;
            }
            catch
            {
                if (ThrowErrors) throw;
                return null;
            }
        }

        public async Task<bool> DeleteOrder(string orderUuid)
        {
            try
            {
                var requestUrl = apiTradingBaseUrl + "orders/" + orderUuid + "/";
                var json = await requestUrl.MakeRequest(null, "DELETE");
                return true;
            }
            catch
            {
                if (ThrowErrors) throw;
                return false;
            }
        }

        public async Task<bool> ActivateOrder(string orderUuid)
        {
            try
            {
                var requestUrl = apiTradingBaseUrl + "orders/" + orderUuid + "/activate";
                var json = await requestUrl.MakeRequest(null, "POST");
                return true;
            }
            catch
            {
                if (ThrowErrors) throw;
                return false;
            }
        }

        public async Task<LemonResult<Order>> GetOrders(OrderSearchFilter filter)
        {
            try
            {
                if (filter == null)
                    throw new Exception("Filter is required");

                var requestUrl = apiTradingBaseUrl + "orders";

                var parameters = new List<string>();

                if (filter.From.HasValue)
                    parameters.Add("from=" + filter.From.ToUnixDt());
                if (filter.To.HasValue)
                    parameters.Add("to=" + filter.To.ToUnixDt());
                if (filter.Side != OrderSide.All)
                    parameters.Add("side=" + filter.Side.ToString().ToLower());
                if (filter.Type != OrderType.All)
                    parameters.Add("type=" + filter.Type.ToString().ToLower());
                if (filter.Isin != null)
                    parameters.Add("isin=" + filter.Isin.ToString());

                if (parameters.Any())
                    requestUrl += "?" + string.Join("&", parameters);

                var result = new LemonResult<Order>();

                var hasNextPage = true;
                while (hasNextPage)
                {
                    var json = await requestUrl.MakeRequest(null, "GET");
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
                if (ThrowErrors) throw;
                return null;
            }
        }

        public async Task<Order> GetOrder(string orderUuid)
        {
            try
            {
                var url = $"{apiTradingBaseUrl}orders/{orderUuid}";
                var json = await url.MakeRequest(null, "GET");

                var response = JsonConvert.DeserializeObject<SingleLemonResult<Order>>(json);
                return response.Result;
            }
            catch
            {
                if (ThrowErrors) throw;
                return null;
            }
        }

        // Stock Market

        public async Task<ChartValue> GetDailyOHLC(string symbol)
        {
            try
            {
                var url = $"{apiDataBaseUrl}ohlc/d1?isin={symbol}";
                var json = await url.MakeRequest(null, "GET");

                var response = JsonConvert.DeserializeObject<LemonResult<ChartValue>>(json);
                return response.Results.FirstOrDefault();
            }
            catch
            {
                if (ThrowErrors) throw;
                return new ChartValue();
            }
        }

        public async Task<(double Ask, double Bid)> GetTicker(string isin)
        {
            try
            {
                string url = $"{apiDataBaseUrl}quotes?isin={isin}";
                var json = await url.MakeRequest(null, "GET");
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
                if (ThrowErrors) throw;
                return (0, 0);
            }
        }

        public async Task<List<ChartValue>> GetChart(string isin, DateTime from, DateTime? to = null)
        {
            var result = new List<ChartValue>();
            try
            {
                to = to ?? DateTime.UtcNow;
                var utcTime = System.Web.HttpUtility.UrlEncode(from.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
                string url = $"{apiDataBaseUrl}ohlc/m1/?mic=XMUN&isin={isin}&from={utcTime}&decimals=true&epoch=false&sorting=asc&limit=250";

                while (from <= to)
                {
                    try
                    {
                        var json = await url.MakeRequest(null, "GET");
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
                            utcTime = System.Web.HttpUtility.UrlEncode(from.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
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
                if (ThrowErrors) throw;
                return null;
            }

            return result.OrderBy(p => p.Created).ToList();
        }

        public async Task<LemonResult<Instrument>> Search(InstrumentSearchFilter filter)
        {
            try
            {
                var resultSet = new LemonResult<Instrument>() { Results = new List<Instrument>() };

                var requestUrl = apiDataBaseUrl + "instruments?";

                var qryStr = new StringBuilder();
                if (filter.SearchByIsins == null || !filter.SearchByIsins.Any())
                    qryStr.Append("search=" + (string.IsNullOrEmpty(filter.SearchByString) ? "" : System.Web.HttpUtility.UrlEncode(filter.SearchByString)));
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
                    var json = await reqUrl.MakeRequest(null, "GET");
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
                if (ThrowErrors) throw;
                return null;
            }
        }

        // Streaming

        public void SubscripeToUserChannel(Action<Message> action)
        {
            try
            {
                var channel = StreamingClient?.Channels.Get(UserId);
                channel?.Subscribe((msg) =>
                {
                    action?.Invoke(msg);
                });

                ChannelCollection.Add(ChannelCollection.Count + 1, channel);
            }
            catch
            {
                if (ThrowErrors) throw;
                return;
            }
        }

        public void SubscribeToIsin(List<string> isins)
        {
            try
            {
                var channel = StreamingClient?.Channels.Get($"{UserId}.subscriptions");
                channel.Publish("isins", string.Join(",", isins));
            }
            catch
            {
                if (ThrowErrors) throw;
                return;
            }
        }

        public void UnsubscripeFromUserChannel()
        {
            try
            {
                var channel = StreamingClient?.Channels.Get(UserId);
                channel.Unsubscribe();
            }
            catch
            {
                if (ThrowErrors) throw;
                return;
            }
        }
    }
}
