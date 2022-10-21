using System;
using LemonMarkets.Helper;
using LemonMarkets.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LemonMarkets.Models
{
    public class Order
    {
        [JsonProperty("isin_title")]
        public string Title { get; set; }

        [JsonProperty("isin")]
        public string Isin { get; set; }

        [JsonConverter(typeof(DoubleDateTimeJsonConverter))]
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

        [JsonProperty("executed_price")] 
        public double? ExecutedPrice { get; set; }

        [JsonConverter(typeof(DoubleDateTimeJsonConverter))]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type")]
        public OrderType Type { get; set; }

        [JsonConverter(typeof(DoubleDateTimeJsonConverter))]
        [JsonProperty("executed_at")]
        public DateTime ExecutedAt { get; set; }

        [JsonProperty("executed_quantity")] 
        public int ExecutedQuantity { get; set; }
    }
}
