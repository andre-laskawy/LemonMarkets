using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LemonMarkets.Models.Responses
{
    public class LemonResults<T> : LemonResult
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
        public List<T> Results
        {
            get; set;
        }

        #endregion get/set
    }
}
