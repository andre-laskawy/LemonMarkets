using LemonMarkets.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LemonMarkets.Helper
{
    public static class LemonMarketsCache
    {
        private static int chartCacheRefreshTime;

        private static Semaphore chartRefresh = new Semaphore(1, 1);

        private static LemonApi api;

        public static ConcurrentDictionary<string, List<ChartValue>> ChartCache { get; set; } = new ConcurrentDictionary<string, List<ChartValue>>();

        /// <summary>
        /// Initializes the Cache.
        /// </summary>
        /// <param name="chartCacheRefresh">The chart cache refresh time in seconds.</param>
        /// <returns></returns>
        public static void Init(int chartCacheRefresh = 15)
        {
            api = new LemonApi();
            chartCacheRefreshTime = chartCacheRefresh;
        }

        public static async Task<List<ChartValue>> GetChart(string symbol, DateTime from)
        {
            var result = new List<ChartValue>();

            try
            {
                chartRefresh.WaitOne();

                if (ChartCache.ContainsKey(symbol))
                {
                    result = ChartCache[symbol];
                }
                else
                {
                    while(!ChartCache.TryAdd(symbol, result))
                    {
                        await Task.Delay(10);
                    }
                }

                // check if chart exists for the lower timeframe with a buffer of 1 minute
                if (!result.Any(p => p.Created <= from))
                {
                    var toDate = result.Any() ? result.Min(p => p.Created).AddSeconds(-1) : DateTime.UtcNow;
                    var chart = await api.GetChart(symbol, from, toDate);
                    if (chart.Any())
                    {
                        result.InsertRange(0, chart);
                    }
                }

                // get new chart values after chache time
                var maxDate = result.Any() ? result.Max(p => p.Created) : DateTime.MinValue;
                if (maxDate < DateTime.UtcNow.AddSeconds(chartCacheRefreshTime * -1))
                {
                    var chart = await api.GetChart(symbol, maxDate.AddSeconds(1));
                    if (chart.Any())
                    {
                        result.AddRange(chart);
                    }
                }
            }
            catch { }
            finally
            {
                chartRefresh.Release();
            }

            return result;
        }
    }
}
