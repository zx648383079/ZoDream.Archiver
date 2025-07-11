using System;
using System.IO;
using System.Text;

namespace ZoDream.Shared.Logging
{
    public class FileLogger(string filePath, LogLevel level = LogLevel.Debug) : ILogger
    {
        private StreamWriter? _writer;

        public LogLevel Level => level;

        private void TryOpen()
        {
            if (_writer is not null)
            {
                return;
            }
            var fs = File.OpenWrite(filePath);
            fs.Seek(0, SeekOrigin.End);
            _writer = new(fs, Encoding.UTF8);
        }

        private void WriteLine(string content)
        {
            TryOpen();
            _writer!.WriteLine(content);
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
            WriteLine($"[{DateTime.Now}] {level}: {message}");
        }

        public void Log(LogLevel level, string message, string source)
        {
            if (level < Level)
            {
                return;
            }
            WriteLine($"[{DateTime.Now}] {level}: {message} in {source}");
        }

        public void Log(Exception message)
        {
            Log(LogLevel.Error, message, string.Empty);
        }

        public void Log(LogLevel level, Exception message, string source)
        {
            if (level < Level)
            {
                return;
            }
            WriteLine($"[{DateTime.Now}] {level}: {message} in {source}");
            if (!string.IsNullOrEmpty(message.Source))
            {
                WriteLine(message.Source);
            }
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

        public void Flush()
        {
            _writer?.Flush();
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _writer = null;
        }

    }
}
