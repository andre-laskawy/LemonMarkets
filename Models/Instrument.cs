using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace LemonMarkets.Models
{
    public class Instrument
    {
        [JsonProperty("isin")]
        public string ISIN { get; set; }

        [JsonProperty("wkn")]
        public string WKN { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InstrumentType InstrumentType { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("tradable")]
        public bool Tradable { get; set; }

        [JsonProperty("trading_venues")]
        public List<TradingVenue> TradingVenues { get; set; } = new List<TradingVenue>();

    }

    public class TradingVenue
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("mic")]
        public string Mic { get; set; }
    }

    public enum InstrumentType
    {
        Stock = 0,
        Bond = 1,
        Fund = 2,
        ETF = 3,
        Warrant = 4
    }
}
