using System;
using LemonMarkets.Models.Enums;

namespace LemonMarkets.Models
{
    public class OrderSearchFilter
    {
        public string? SpaceUuid
        {
            get; set;
        }

        public OrderSide Side
        {
            get; set;
        }

        public DateTime? From
        {
            get; set;
        }

        public DateTime? To
        {
            get; set;
        }

        public OrderType Type
        {
            get; set;
        }

        public OrderStatus Status
        {
            get; set;
        }

        public bool WithPaging
        {
            get; set;
        }

        public string? Isin
        {
            get; set;
        }
    }
}
