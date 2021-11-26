using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LemonMarkets.Models.Responses
{
    public class LemonResults<T,B> : LemonResult
    {
        #region get/set

        [JsonPropertyName("previous")]
        public string Previous
        {
            get; set;
        }

        [JsonPropertyName("next")]
        public string Next
        {
            get; set;
        }

        [JsonPropertyName("results")]
        public Dictionary<T, B> Results
        {
            get; set;
        }

        #endregion get/set
    }
}
