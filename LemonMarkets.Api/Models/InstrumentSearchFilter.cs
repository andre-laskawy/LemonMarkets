using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using LemonMarkets.Models.Enums;

namespace LemonMarkets.Models
{
    public class InstrumentSearchFilter
    {
        public string SearchByString { get; set; }
        public List<string> SearchByIsins { get; set; }
        public InstrumentType? InstrumentType { get; set; }
        public Enums.TradingVenue? TradingVenue { get; set; }
        public Currency? Currency { get; set; }
        public bool? IsTradable { get; set; }
        public bool WithPaging { get; set; }

    }
}
