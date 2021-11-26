using System;
using LemonMarkets.Models.Enums;

namespace LemonMarkets.Models
{
    public class PostOrderQuery
    {
        public string SpaceUuid { get; set; }

        public string Isin { get; set; }

        public DateTime ValidUntil { get; set; }

        public OrderSide Side { get; set; }

        public int Quantity { get; set; }

        public double? StopPrice { get; set; }

        public double? LimitPrice { get; set; }
    }
}
