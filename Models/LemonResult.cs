using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LemonMarkets.Models
{
    public class LemonResult<T> where T: class
    {
        [JsonProperty("previous")]
        public string Previous { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("results")]
        public List<T> Results { get; set; } = new List<T>();
    }
}
