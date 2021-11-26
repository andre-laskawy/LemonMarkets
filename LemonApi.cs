using System.Globalization;
using LemonMarkets.Models.Enums;
using LemonMarkets.Interfaces;
using LemonMarkets.Models;
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
using WsApiCore;
using System.Security.Cryptography.X509Certificates;
using LemonMarkets.Repos.V1;

namespace LemonMarkets
{

    /// <summary>
    /// Defines the <see cref="LemonApi" />.
    /// </summary>
    public class LemonApi : ILemonApi
    {

        #region vars

        private static Semaphore semaphore = new Semaphore(1, 1);

        private static string apiDataBaseUrl = "https://data.lemon.markets";
        
        private static string apiPaperTradingBaseUrl = "https://paper-trading.lemon.markets";

        private static string apiRealTradingBaseUrl = "https://trading.lemon.markets";

        #endregion vars

        #region get/set

        public ConnectionInfo ConnectionInfo
        {
            get;
        }

        public string ApiKey
        {
            get;
        }

        internal WsAPICore TradingApi
        {
            get;
        }

        internal WsAPICore MarketDataApi
        {
            get;
        }

        public IOrdersRepo Orders
        {
            get;
        }

        public ISpacesRepo Spaces
        {
            get;
        }

        #endregion get/set

        #region ctor

        public LemonApi(string apiKey, ConnectionInfo connectionInfo)
        {
            this.ConnectionInfo = connectionInfo;
            this.ApiKey = apiKey;

            this.TradingApi = new WsAPICore(connectionInfo.TradingAdress, "v1");
            this.TradingApi.CheckCertEasy += Api_CheckCertEasy;
            this.TradingApi.SetNewAuth ( apiKey );
            this.MarketDataApi = new WsAPICore(connectionInfo.MarketDataAdress, "v1");
            this.MarketDataApi.CheckCertEasy += Api_CheckCertEasy;
            this.MarketDataApi.SetNewAuth ( apiKey );

            this.Orders = new OrdersRepo(this.TradingApi);
            this.Spaces = new SpaceRepo ( this.TradingApi );
        }

        #endregion ctor

        #region methods

        public static ILemonApi Build(string apiKey, MoneyTradingMode mode)
        {
            string tradingUrl = mode == MoneyTradingMode.Paper ? apiPaperTradingBaseUrl : apiRealTradingBaseUrl;

            ConnectionInfo connectionInfo = new ConnectionInfo(apiDataBaseUrl, tradingUrl);

            return new LemonApi(apiKey, connectionInfo);
        }

        #endregion methods

        #region events

        private bool Api_CheckCertEasy(string hostname, X509Certificate2 x509Certificate2, X509Chain x509Chain)
        {
            if (x509Chain.ChainStatus.Any(status => status.Status == X509ChainStatusFlags.UntrustedRoot)) return false;//Assert.Fail("certifcate has no trusted root");
            //if (!x509Certificate2.Subject.Contains(string.Format("CN={0}", hostname))) return false;//Assert.Fail("Hostname of the certificate not matched");

            return true;
        }

        #endregion events

        /*public async Task<ChartValue> GetDailyOHLC(string symbol)
        {
            try
            {
                var url = $"{apiDataBaseUrl}ohlc/d1?isin={symbol}";
                var json = await MakeRequest(url, null, "GET");

                var response = JsonConvert.DeserializeObject<LemonResults<ChartValue>>(json);
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

                while (currentDT <= to)
                {
                    try
                    {
                        var json = await MakeRequest(url, null, "GET");
                        var response = JsonConvert.DeserializeObject<LemonResults<ChartValue>>(json);

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

        public async Task<LemonResults<Instrument>> Search(InstrumentSearchFilter filter)
        {
            try
            {
                var resultSet = new LemonResults<Instrument>(){Results = new List<Instrument>()};

                var requestUrl = apiDataBaseUrl + "instruments?";
                
                var qryStr = new StringBuilder();
                if (filter.SearchByIsins == null || !filter.SearchByIsins.Any())
                    qryStr.Append("search=" + (string.IsNullOrEmpty(filter.SearchByString) ? "" : HttpUtility.UrlEncode(filter.SearchByString)));
                else
                    qryStr.Append("isin=" + string.Join(",", filter.SearchByIsins));

                //if (filter.TradingVenue.HasValue)
                //    qryStr.Append("&mic=" + filter.TradingVenue.GetValueOrDefault());
                    
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
                    var results = JsonConvert.DeserializeObject<LemonResults<Instrument>>(json);
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
        }*/

        #region enums

        public enum MoneyTradingMode
        {
            Paper,
            Real
        }

        #endregion enums

    }
}
