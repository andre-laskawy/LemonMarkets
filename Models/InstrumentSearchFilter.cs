using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace LemonMarkets.Models
{
    public class InstrumentSearchFilter
    {
        public string SearchByString { get; set; }
        public List<string> SearchByIsins { get; set; }
        public InstrumentType? InstrumentType { get; set; }
        public TradingVenueEnum? TradingVenue { get; set; }
        public CurrencyEnum? Currency { get; set; }
        public bool? IsTradable { get; set; }
        public bool WithPaging { get; set; }

        public enum CurrencyEnum
        {
            EUR = 1,
            USD = 2
        }

        public enum TradingVenueEnum
        {
            XMUN = 1
        }
    }
}
