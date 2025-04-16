using System;
using System.IO;
using System.Text;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Logging
{
    public class FileLogger : ILogger
    {
        public FileLogger(Stream output, LogLevel level = LogLevel.Debug)
        {
            _writer = new StreamWriter(output, Encoding.UTF8);
            Level = level;
        }
        private readonly StreamWriter _writer;
        public LogLevel Level { get; private set; }

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
            _writer.WriteLine($"[{DateTime.Now}] {level}: {message}");
        }

        public void Progress(long current, long total)
        {
            Progress(current, total, string.Empty);
        }

        public void Progress(long current, long total, string message)
        {

        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
