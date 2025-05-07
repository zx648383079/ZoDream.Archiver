using System;

namespace ZoDream.Shared.Logging
{
    public interface ILogger : IDisposable
    {

        public void Log(LogLevel level, string message, string source);
        public void Log(LogLevel level, Exception message, string source);
        public void Log(Exception message);
        public void Log(LogLevel level, string message);
        public void Log(string message);

        public void Debug(string message);

        public void Info(string message);

        public void Warning(string message);

        public void Error(string message);

        /// <summary>
        /// 进度
        /// </summary>
        /// <param name="current"></param>
        /// <param name="total"></param>
        public void Progress(long current, long total = 0);

        public ProgressLogger CreateProgress(string title, long max = 0);
        public ProgressLogger CreateSubProgress(string title, long max = 0);

        public void Flush();

    }
}
