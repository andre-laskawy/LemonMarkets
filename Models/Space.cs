using System.Text.Json.Serialization;

namespace LemonMarkets.Models
{
    public class Space
    {
        [JsonPropertyName("uuid")] 
        public string Uuid
        {
            get; set;
        }

        [JsonPropertyName("name")] 
        public string Name
        {
            get; set;
        }

        [JsonPropertyName("state")] 
        public State State
        {
            get; set;
        }

        [JsonPropertyName("type")] 
        public string Type
        {
            get; set;
        }
    }


    public class State
    {
        [JsonPropertyName("balance")]
        public string Balance
        {
            get; set;
        }

        [JsonPropertyName("cash_to_invest")] 
        public string CashToInvest
        {
            get; set;
        }
    }
}
