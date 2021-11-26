using System;
using System.Text.Json.Serialization;
using LemonMarkets.Models.Enums;

namespace LemonMarkets.Models
{
    public class PostedOrder
    {
        [JsonPropertyName("isin")] 
        public string Isin
        {
            get; set;
        }

        //[JsonConverter(typeof(DoubleDateTimeJsonConverter))]
        [JsonPropertyName("valid_until")]
        public DateTime ValidUntil
        {
            get; set;
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("side")]
        public OrderSide Side
        {
            get; set;
        }

        [JsonPropertyName("quantity")] 
        public int Quantity
        {
            get; set;
        }

        [JsonPropertyName("stop_price")] 
        public double? StopPrice
        {
            get; set;
        }

        [JsonPropertyName("limit_price")] 
        public double? LimitPrice
        {
            get; set;
        }

        [JsonPropertyName("uuid")] 
        public string Uuid
        {
            get; set;
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        public OrderStatus Status
        {
            get; set;
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("trading_venue_mic")] 
        public Enums.TradingVenue Venue
        {
            get; set;
        }
    }
}