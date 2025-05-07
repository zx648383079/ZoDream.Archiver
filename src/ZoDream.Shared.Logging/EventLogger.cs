using System;

namespace ZoDream.Shared.Logging
{
    public class EventLogger(LogLevel level = LogLevel.Debug) : ILogger
    {
        public EventLogger(ILogger target, LogLevel level = LogLevel.Debug)
            : this(level)
        {
            _target = target;
        }
        /// <summary>
        /// 附加一个线程
        /// </summary>
        private readonly ILogger? _target;
        private ProgressLogger? _master;
        private ProgressLogger? _child;

        public LogLevel Level => level;

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

        public void Warning(string message)
        {
            Log(LogLevel.Warn, message);
        }

        public void Log(string message)
        {
            Log(Level, message);
        }

        public void Log(LogLevel level, string message)
        {
            _target?.Log(level, message);
            if (level >= Level)
            {
                OnLog?.Invoke(message, level);
            }
        }

        public void Log(LogLevel level, string message, string source)
        {
            _target?.Log(level, message, source);
            if (level >= Level)
            {
                OnLog?.Invoke(message, level);
            }
        }

        public void Log(Exception message, string source)
        {
            Log(LogLevel.Error, message, source);
        }

        public void Log(LogLevel level, Exception message, string source)
        {
            _target?.Log(level, message, source);
            if (level >= Level)
            {
                OnLog?.Invoke(message.ToString(), level);
            }
        }

        public void Progress(long current, long total)
        {
            _master ??= new ProgressLogger(this, true);
            _master.Max = total;
            _master.Value = current;
            OnProgress?.Invoke(_master);
        }


        public ProgressLogger CreateProgress(string title, long max = 0)
        {
            _master ??= new ProgressLogger(this, true);
            _master.Title = title;
            _master.Max = max;
            _master.Value = 0;
            OnProgress?.Invoke(_master);
            return _master;
        }

        public ProgressLogger CreateSubProgress(string title, long max = 0)
        {
            _child ??= new ProgressLogger(this, false);
            _child.Title = title;
            _child.Max = max;
            _child.Value = 0;
            OnProgress?.Invoke(_child);
            return _child;
        }

        public void Flush()
        {

        }

        internal void Emit(ProgressLogger progress)
        {
            OnProgress?.Invoke(progress);
        }

        public void Dispose()
        {
            _target?.Dispose();
        }


    }

    public delegate void LogEventHandler(string message, LogLevel level);
    public delegate void ProgressEventHandler(ProgressLogger progress);
}
