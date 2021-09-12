using System;
using System.Collections.Generic;
using System.Text;

namespace LemonMarkets.Models.Enums
{
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
