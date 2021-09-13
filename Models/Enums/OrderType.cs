using System;
using System.Collections.Generic;
using System.Text;

namespace LemonMarkets.Models.Enums
{
    public enum OrderType
    {
        All = 0,
        Limit = 1,
        Market = 2,
        Stop_Limit = 3,
        Stop_Market = 4
    }
}
