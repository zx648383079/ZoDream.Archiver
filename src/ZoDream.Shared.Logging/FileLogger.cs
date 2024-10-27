using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Logging
{
    public class FileLogger : ILogger
    {
        public LogLevel Level => LogLevel.Debug;

        public FileLogger()
        {

        }

        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }


        public void Warning(string message)
        {
            Log(LogLevel.Warn, message);
        }

        public void Log(string message)
        {
            if (Level != LogLevel.Debug)
            {
                return;
            }
            Log(Level, message);
        }

        public void Log(LogLevel level, string message)
        {
            if (level < Level)
            {
                return;
            }

        }

        public void Progress(long current, long total)
        {
            Progress(current, total, string.Empty);
        }

        public void Progress(long current, long total, string message)
        {

        }

    }
}
