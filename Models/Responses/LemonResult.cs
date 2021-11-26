using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonMarkets.Models.Responses
{
    public class LemonResult
    {

        #region get/set

        public DateTime Time
        {
            get;
            set;
        }

        public string Status
        {
            get;
            set;
        }

        public bool IsOk
        {
            get;
            set;
        }

        #endregion get/set

    }
}
