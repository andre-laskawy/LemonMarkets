using System;
using System.Text.Json.Serialization;

namespace LemonMarkets.Models
{
    public class ChartValue
    {
        [JsonPropertyName("o")]
        public double Open
        {
            get; set;
        }

        [JsonPropertyName("c")]
        public double Close
        {
            get; set;
        }

        [JsonPropertyName("h")]
        public double High
        {
            get; set;
        }

        [JsonPropertyName("l")]
        public double Low
        {
            get; set;
        }

        [JsonPropertyName("t")]
        public string Timestamp
        {
            get; set;
        }

        [JsonIgnore]
        public DateTime Created
        {
            get; set;
        }

        [JsonIgnore]
        public string Symbol
        {
            get; set;
        }
    }
}
