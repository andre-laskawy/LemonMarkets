using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonMarkets.Models.Requests.Trading
{
    public class RequestActivateOrder
    {

        #region get/set

        public string OrderId
        {
            get;
            set;
        }

        public string Pin
        {
            get;
            set;
        }

        #endregion get/set

        #region ctor

        public RequestActivateOrder(string orderId, string pin = null)
        {
            this.OrderId = orderId;
            this.Pin = pin;
        }

        #endregion ctor

    }
}
