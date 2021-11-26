using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WsApiCore
{
    public class FileUploadRequest
    {

        public FileInfo File
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

    }

}
