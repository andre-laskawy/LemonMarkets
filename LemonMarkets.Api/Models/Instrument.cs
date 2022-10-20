using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using LemonMarkets.Models.Enums;

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

    
}
