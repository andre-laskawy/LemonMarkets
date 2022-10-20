using System;
using LemonMarkets.Models.Enums;

namespace LemonMarkets.Models
{
    public class OrderSearchFilter
    {
        public string Isin { get; set; }

        public OrderSide Side { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public OrderType Type { get; set; }

        public bool WithPaging { get; set; }
    }
}
