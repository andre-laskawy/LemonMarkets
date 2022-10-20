using System;
using LemonMarkets.Helper;
using LemonMarkets.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LemonMarkets.Models
{
    public class Order
    {
        [JsonProperty("instrument")] 
        public InstrumentShort Instrument { get; set; }

        [JsonConverter(typeof(DoubleDateTimeJsonConverter))]
        [JsonProperty("valid_until")]
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

        [JsonProperty("uuid")] 
        public string Uuid { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public OrderStatus Status { get; set; }

        [JsonProperty("average_price")] 
        public double? AveragePrice { get; set; }

        [JsonConverter(typeof(DoubleDateTimeJsonConverter))]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type")]
        public OrderType Type { get; set; }

        [JsonConverter(typeof(DoubleDateTimeJsonConverter))]
        [JsonProperty("processed_at")]
        public DateTime ProcessedAt { get; set; }

        [JsonProperty("processed_quantity")] 
        public int ProcessedQuantity { get; set; }
    }

    public class InstrumentShort
    {
        [JsonProperty("title")] 
        public string Title { get; set; }

        [JsonProperty("isin")] 
        public string Isin { get; set; }
    }

}
