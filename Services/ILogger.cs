using LemonMarkets.Models;
using System;

namespace LemonMarkets.Services
{
    public interface ILemonLogger
    {
        void Log(LogLevel level, string log);

        void Log(Exception ex);
    }
}
