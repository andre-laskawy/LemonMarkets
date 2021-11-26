using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LemonMarkets.Models.Enums;

namespace LemonMarkets.Models.Requests.Trading
{

    public class RequestSpace
    {

        #region get/set

        /// <summary>
        /// Name of your new Space
        /// </summary>
        [Required]
        public string Name
        {
            get;
        }

        /// <summary>
        /// Space type - auto or manual
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public SpaceType Type
        {
            get;
        }

        /// <summary>
        /// Risk limit of your new Space.
        /// Please see here for information on the numbers format in the Trading API.
        /// 1€ == 10000
        /// </summary>
        [Required]
        public long Risk_limit
        {
            get;
        }

        /// <summary>
        /// Description of your new Space
        /// </summary>
        public string? Description
        {
            get;
        }

        #endregion get/set

        #region ctor

        public RequestSpace ( string name, SpaceType type, long riskLimit, string? description = null )
        {
            this.Name = name;
            this.Type = type;
            this.Risk_limit = riskLimit;
            this.Description = description;
        }

        #endregion ctor

    }

}