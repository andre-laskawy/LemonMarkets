using Newtonsoft.Json;

namespace LemonMarkets.Models
{
        public class Order
        {
        [JsonProperty("instrument")]  public InstrumentShort Instrument { get; set; }
        [JsonProperty("valid_until")] public string ValidUntil { get; set; }
        [JsonProperty("side")] public string Side { get; set; }
        [JsonProperty("quantity")] public int Quantity { get; set; }
        [JsonProperty("stop_price")] public string StopPrice { get; set; }
        [JsonProperty("limit_price")] public string LimitPrice { get; set; }
        [JsonProperty("uuid")] public string Uuid { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("average_price")] public string AveragePrice { get; set; }
        [JsonProperty("created_at")] public string CreatedAt { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("processed_at")] public string ProcessedAt { get; set; }
        [JsonProperty("processed_quantity")] public int ProcessedQuantity { get; set; }
        }

        public class InstrumentShort
        {
            [JsonProperty("title")] public string title { get; set; }
            [JsonProperty("isin")] public string isin { get; set; }
        }

    }
