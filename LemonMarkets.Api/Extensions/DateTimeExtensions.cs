using System;

namespace LemonMarkets.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToUnixDt(this DateTime? dt)
        {
            return !dt.HasValue ? 0 : ToUnixDt(dt.GetValueOrDefault());
        }

        public static long ToUnixDt(this DateTime dt)
        {
            var val = (long)dt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            return val;
        }
    }
}
