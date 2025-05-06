using System;
using System.IO;
using System.Text;

namespace ZoDream.Shared.Logging
{
    public class FileLogger(Stream output, LogLevel level = LogLevel.Debug) : ILogger
    {
        private readonly StreamWriter _writer = new(output, Encoding.UTF8);

        public LogLevel Level => level;


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

        public void Log(LogLevel level, string message, string source)
        {
            if (level < Level)
            {
                return;
            }
            _writer.WriteLine($"[{DateTime.Now}] {level}: {message}; {source}");
        }

        public void Log(LogLevel level, Exception message, string source)
        {
            if (level < Level)
            {
                return;
            }
            _writer.WriteLine($"[{DateTime.Now}] {level}: {message}; {source}");
        }



        public void Dispose()
        {
            _writer.Dispose();
        }

    

        public void Progress(long current, long total)
        {

        }

        public ProgressLogger CreateProgress(string title, long max = 0)
        {
            return new ProgressLogger(this, true)
            {
                Title = title,
                Max = max
            };
        }

        public ProgressLogger CreateSubProgress(string title, long max = 0)
        {
            return new ProgressLogger(this, false)
            {
                Title = title,
                Max = max
            };
        }

        
    }
}
