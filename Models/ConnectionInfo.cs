using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonMarkets.Models
{
    public class ConnectionInfo
    {

        #region get/set

        public string MarketDataAdress
        {
            get;
        }

        public string TradingAdress
        {
            get;
        }

        #endregion get/set

        #region ctor

        public ConnectionInfo(string marketDataAdress, string tradingAdress)
        {
            this.MarketDataAdress = marketDataAdress;
            this.TradingAdress = tradingAdress;
        }

        #endregion ctor

    }
}
