using LemonMarkets.Models;
using Skender.Stock.Indicators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonMarkets.Helper
{
    public static class IndicatorHelper
    {
        public static int CacheTimeInMinutes { get; set; } = 60 * 6;

        public static ConcurrentDictionary<string, LemonIndicators> Cache { get; set; } = new ConcurrentDictionary<string, LemonIndicators>();

        public static async Task<LemonIndicators> GetIndicators(string isin, Func<Task<List<ChartValue>>> valueFunc, int periodTimeInMinutes)
        {
            try
            {
                if(Cache.ContainsKey(isin)
                    && Cache[isin].Created > DateTime.UtcNow.AddMinutes(CacheTimeInMinutes * -1))
                {
                    return Cache[isin];
                }

                var indicators = new LemonIndicators()
                {
                    Created = DateTime.UtcNow,
                };

                var chartData = await valueFunc.Invoke();
                indicators.ADX = GetADX(chartData, periodTimeInMinutes);
                indicators.MACD = GetMACD(chartData, periodTimeInMinutes);
                indicators.RSI = GetRSI(chartData, periodTimeInMinutes);
                indicators.SMA3 = GetSMA(chartData, periodTimeInMinutes, 3);
                indicators.SMA5 = GetSMA(chartData, periodTimeInMinutes, 5);
                indicators.SMA10 = GetSMA(chartData, periodTimeInMinutes, 10);
                indicators.SMA20 = GetSMA(chartData, periodTimeInMinutes, 20);
                indicators.SMA30 = GetSMA(chartData, periodTimeInMinutes, 30);

                if (Cache.ContainsKey(isin))
                {
                    while (!Cache.TryUpdate(isin, indicators, Cache[isin]))
                    {
                        await Task.Delay(50);
                    }
                }
                else
                {
                    while (!Cache.TryAdd(isin, indicators))
                    {
                        await Task.Delay(50);
                    }
                }
              
                return indicators;
            }
            catch
            {
                return new LemonIndicators();
            }
        
        }

        private static double GetSMA(List<ChartValue> values, int periodTimeInMinutes, int period)
        {
            try
            {
                var quotes = ParseToQuotes(values, periodTimeInMinutes);
                var result = quotes.GetSma(period);
                return (double)(result.LastOrDefault().Sma ?? 0);
            }
            catch
            {
                return 0;
            }
        }

        private static double GetRSI(List<ChartValue> values, int periodTimeInMinutes)
        {
            try
            {
                var quotes = ParseToQuotes(values, periodTimeInMinutes);
                var result = quotes.GetRsi();
                return (double)(result.LastOrDefault().Rsi ?? 0);
            }
            catch 
            {
                return 0;
            }            
        }

        private static double GetMACD(List<ChartValue> values, int periodTimeInMinutes)
        {
            try
            {
                var quotes = ParseToQuotes(values, periodTimeInMinutes);
                var result = quotes.GetMacd();
                return (double)(result.LastOrDefault().Macd ?? 0);
            }
            catch
            {
                return 0;
            }
        }

        private static double GetADX(List<ChartValue> values, int periodTimeInMinutes)
        {
            try
            {
                var quotes = ParseToQuotes(values, periodTimeInMinutes);
                var result = quotes.GetAdx();
                return (double)(result.LastOrDefault().Adx ?? 0);
            }
            catch
            {
                return 0;
            }
        }

        private static List<Quote> ParseToQuotes(List<ChartValue> chartValues, int periodTimeInMinutes)
        {
            var quotes = new List<Quote>();

            var chartList = chartValues.OrderBy(p => p.Created).ToList();
            var lastDate = chartList.FirstOrDefault().Created;
            for (int i = 0; i < chartList.Count(); i++)
            {
                var date = chartList[i].Created;
                if (date > lastDate.AddMinutes(periodTimeInMinutes))
                {
                    lastDate = date;
                    quotes.Add(new Quote()
                    {
                        Close = (decimal)chartList[i - 1].Close,
                        Date = chartList[i - 1].Created,
                        High = (decimal)chartList[i - 1].High,
                        Low = (decimal)chartList[i - 1].Low,
                        Open = (decimal)chartList[i - 1].Open,
                    });
                }
            }

            return quotes;
        }
    }
}
