using Newtonsoft.Json;

namespace LemonMarkets.Models
{
    public class Space
    {
        [JsonProperty("uuid")] 
        public string Uuid { get; set; }

        [JsonProperty("name")] 
        public string Name { get; set; }

        [JsonProperty("state")] 
        public State State { get; set; }

        [JsonProperty("type")] 
        public string Type { get; set; }
    }


    public class State
    {
        [JsonProperty("balance")]
        public string Balance { get; set; }

        [JsonProperty("cash_to_invest")] 
        public string CashToInvest { get; set; }
    }
}
