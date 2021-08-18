using Newtonsoft.Json;
using System;

namespace LemonMarkets.Models
{
    public class ChartValue
    {
        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("t")]
        public string Timestamp { get; set; }

        [JsonIgnore]
        public DateTime Created { get; set; }

        [JsonIgnore]
        public string Symbol { get; set; }
    }
}
