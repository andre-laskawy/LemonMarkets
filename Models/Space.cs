using System.Text.Json.Serialization;
using LemonMarkets.Models.Enums;

namespace LemonMarkets.Models
{
    public class Space
    {
        
        /// <summary>
        /// Space ID
        /// </summary>
        public string Id
        {
            get; set;
        }

        /// <summary>
        /// Name of Space
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Description of Space
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Space Type: Paper Money or Real Money
        /// </summary>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SpaceType Type
        {
            get; set;
        }

        /// <summary>
        /// Amount of Risk Limit
        /// </summary>
        public int Risk_limit
        {
            get;
            set;
        }

        /// <summary>
        /// Your current Profit
        /// </summary>
        public int Earnings
        {
            get;
            set;
        }

        /// <summary>
        /// Risk Limit - Backfire - pending Orders
        /// </summary>
        public int Buying_power
        {
            get;
            set;
        }

        /// <summary>
        /// Combined Expenses and Losses
        /// </summary>
        public int Backfire
        {
            get;
            set;
        }

        /// <summary>
        /// Real Money Spaces only: potential linked Paper Money Space
        /// </summary>
        public string Linked
        {
            get;
            set;
        }
    }

}
