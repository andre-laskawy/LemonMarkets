using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LemonMarkets.Models.Responses
{
    public class LemonResult<T> : LemonResult
    {

        #region get/set

        [JsonPropertyName("results")]
        public T? Results
        {
            get; set;
        }

        #endregion get/set

        #region ctor

        public LemonResult()
        {

        }

        public LemonResult(string status) : base(status)
        {

        }

        #endregion ctor

    }
}
