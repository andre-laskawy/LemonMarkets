using System;
using System.Collections.Generic;
using System.Text;

namespace LemonMarkets.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToUnixDt(this DateTime? dt)
        {
            if (!dt.HasValue)
                return 0;
            var val = (long) dt.GetValueOrDefault().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            return val;
        }
    }
}
