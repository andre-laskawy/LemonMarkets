using LemonMarkets.Models;
using LemonMarkets.Services;
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

        private static LemonApi api;

        public static ConcurrentDictionary<string, List<ChartValue>> ChartCache { get; set; } = new ConcurrentDictionary<string, List<ChartValue>>();

        public static ConcurrentDictionary<string, Semaphore> Semaphores { get; set; } = new ConcurrentDictionary<string, Semaphore>();

        public static ILemonLogger logger;

        /// <summary>
        /// Initializes the Cache.
        /// </summary>
        /// <param name="chartCacheRefresh">The chart cache refresh time in seconds.</param>
        /// <returns></returns>
        public static void Init(int chartCacheRefresh = 15, ILemonLogger lemonLogger = null)
        {
            api = new LemonApi();
            chartCacheRefreshTime = chartCacheRefresh;
            logger = lemonLogger;
        }

        public static async Task<List<ChartValue>> GetChart(string symbol, DateTime from)
        {
            var result = new List<ChartValue>();

            try
            {
                // lock chart specific refresh via semaphore
                Semaphore semaphore = null;
                if (Semaphores.ContainsKey(symbol))
                {
                    semaphore = Semaphores[symbol];
                    semaphore.WaitOne();
                }
                else
                {
                    while (!Semaphores.TryAdd(symbol, new Semaphore(1, 1)))
                    {
                        await Task.Delay(10);
                    }

                    semaphore = Semaphores[symbol];
                    semaphore.WaitOne();
                }

                if (ChartCache.ContainsKey(symbol))
                {
                    logger?.Log(LogLevel.DEBUG, $"Found cache for: {symbol}");
                    result = ChartCache[symbol]; 
                }
                else
                {
                    while(!ChartCache.TryAdd(symbol, result))
                    {
                        await Task.Delay(10);
                    }

                    logger?.Log(LogLevel.DEBUG, $"Created cache for: {symbol}");
                }

                try
                {
                    // check if chart exists for the lower timeframe with a buffer of 1 minute
                    if (!result.Any(p => p.Created <= from))
                    {
                        var toDate = result.Any() ? result.Min(p => p.Created).AddSeconds(-1) : DateTime.UtcNow;

                        logger?.Log(LogLevel.DEBUG, $"Look up data for {symbol} from {from} - {toDate}");
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

                        logger?.Log(LogLevel.DEBUG, $"Look up data for {symbol} from {from} to now");
                        if (chart.Any())
                        {
                            result.AddRange(chart);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.Log(ex);
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                logger?.Log(ex);
            }

            return result;
        }
    }
}
