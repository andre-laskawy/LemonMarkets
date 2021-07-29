using LemonMarkets.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LemonMarkets.Services
{
    public interface ILemonLogger
    {
        void Log(LogLevel level, string log);

        void Log(Exception ex);
    }
}
