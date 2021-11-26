using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WsApiCore
{
    public interface IResponseFileDownload
    {

        Stream Result
        {
            get;
            set;
        }

        bool IsOk
        {
            get;
            set;
        }

    }

}
