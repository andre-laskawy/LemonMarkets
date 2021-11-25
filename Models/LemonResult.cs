using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LemonMarkets.Models
{
    public class LemonResult<T> where T: class
    {
        [JsonPropertyName("previous")]
        public string Previous { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; }

        [JsonPropertyName("results")]
        public List<T> Results { get; set; } = new List<T>();
    }
}
