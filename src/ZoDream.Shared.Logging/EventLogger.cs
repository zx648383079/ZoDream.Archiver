using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Logging
{
    public class EventLogger(LogLevel level) : ILogger
    {
        public EventLogger() : this(LogLevel.Debug)
        {

        }

        public LogLevel Level { get; private set; } = level;

        public event LogEventHandler? OnLog;
        public event ProgressEventHandler? OnProgress;

        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void Log(string message)
        {
            Log(Level, message);
        }

        public void Log(LogLevel level, string message)
        {
            if (level >= Level)
            {
                OnLog?.Invoke(message, level);
            }
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Progress(long current, long total)
        {
            Progress(current, total, string.Empty);
        }

        public void Warning(string message)
        {
            Log(LogLevel.Warn, message);
        }

        public void Progress(long current, long total, string message)
        {
            OnProgress?.Invoke(current, total, message);
        }

    }

    public delegate void LogEventHandler(string message, LogLevel level);
    public delegate void ProgressEventHandler(long current, long total, string message);
}
