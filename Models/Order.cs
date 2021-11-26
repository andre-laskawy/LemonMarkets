using System;
using System.Text.Json.Serialization;
using LemonMarkets.Models.Enums;

namespace LemonMarkets.Models
{
    public class Order
    {
        [JsonPropertyName("instrument")] 
        public InstrumentShort Instrument
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

        [JsonPropertyName("average_price")] 
        public double? AveragePrice
        {
            get; set;
        }

        //[JsonConverter(typeof(DoubleDateTimeJsonConverter))]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt
        {
            get; set;
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("type")]
        public OrderType Type
        {
            get; set;
        }

        //[JsonConverter(typeof(DoubleDateTimeJsonConverter))]
        [JsonPropertyName("processed_at")]
        public DateTime ProcessedAt
        {
            get; set;
        }

        [JsonPropertyName("processed_quantity")] 
        public int ProcessedQuantity
        {
            get; set;
        }
    }

    public class InstrumentShort
    {
        [JsonPropertyName("title")] 
        public string Title
        {
            get; set;
        }

        [JsonPropertyName("isin")] 
        public string Isin
        {
            get; set;
        }
    }

}
