using System;

namespace LemonMarkets.Models
{
    public class OrderSearchFilter
    {
        public string SpaceUuid { get; set; }
        public OrderSide Side { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public OrderType Type { get; set; }
        public bool WithPaging { get; set; }
        

        public enum OrderSide
        {
            All = 0,
            Buy = 1,
            Sell = 2
        }

        public enum OrderType
        {
            All = 0,
            Limit = 1,
            Market = 2,
            Stop_Limit = 3,
            Stop_Market = 4
        }

        public enum OrderStatus
        {
            All = 0,
            Inactive = 1,
            Active = 2,
            In_Progress = 3,
            Executed = 4,
            Deleted = 5,
            Expired = 6
        }
    }
}
