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
        Activated = 3,
        In_Progress = 4,
        Executed = 5,
        Deleted = 6,
        Expired = 7
    }
}
