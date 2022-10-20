using System;
using LemonMarkets.Helper;
using LemonMarkets.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LemonMarkets.Models
{
    public class PostedOrder
    {
        [JsonProperty("isin")]
        public string Isin { get; set; }

        [JsonProperty("expires_at")]
        public DateTime ValidUntil { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("side")]
        public OrderSide Side { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("stop_price")]
        public double? StopPrice { get; set; }

        [JsonProperty("limit_price")]
        public double? LimitPrice { get; set; }

        [JsonProperty("id")]
        public string Uuid { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public OrderStatus Status { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("venue")]
        public Enums.TradingVenue Venue { get; set; }
    }
}