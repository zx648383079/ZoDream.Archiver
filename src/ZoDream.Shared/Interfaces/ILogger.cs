using ZoDream.Shared.Models;

namespace ZoDream.Shared.Interfaces
{
    public interface ILogger
    {

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
        public void Progress(long current, long total);

        public void Progress(long current, long total, string message);

    }
}
