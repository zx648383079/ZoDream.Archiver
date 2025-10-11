using ZoDream.Shared.Logging;

namespace ZoDream.Archiver
{
    internal class ConsoleLogger(LogLevel basicLevel = LogLevel.Debug) : ILogger
    {
        private static readonly char[] _spinner = ['|', '/', '-', '\\'];
        private readonly ConsoleColor _defaultColor = Console.ForegroundColor;
        private int _counter = 0;
        private ProgressLogger? _master;
        private ProgressLogger? _child;

        public LogLevel Level => basicLevel;

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void Warning(string message)
        {
            Log(LogLevel.Warn, message);
        }

        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void Log(Exception message)
        {
            Log(LogLevel.Error, message, string.Empty);
        }

        public void Log(LogLevel level, string message, string source)
        {
            if (level < basicLevel)
            {
                return;
            }
            var label = $"[{DateTime.Now:HH:mm:ss}]{level}:";
            Console.Write(label);
            SetColor(level);
            Console.WriteLine(message);
            Console.ForegroundColor = _defaultColor;
            if (level >= LogLevel.Warn && !string.IsNullOrWhiteSpace(source))
            {
                Console.Write(new string(' ', label.Length));
                Console.WriteLine(source);
            }
        }

        public void Log(LogLevel level, Exception message, string source)
        {
            if (level < basicLevel)
            {
                return;
            }
            var label = $"[{DateTime.Now:HH:mm:ss}]{level}:";
            Console.Write(label);
            SetColor(level);
            Console.WriteLine(message);
            Console.ForegroundColor = _defaultColor;
            if (level >= LogLevel.Warn && !string.IsNullOrWhiteSpace(source))
            {
                Console.Write(new string(' ', label.Length));
                Console.WriteLine(source);
            }
        }

        public void Log(LogLevel level, string message)
        {
            Log(level, message, string.Empty);
        }

        public void Log(string message)
        {
            Log(Level, message);
        }

        public void Progress(long current, long total = 0)
        {
            _master ??= new ProgressLogger(this, true);
            _master.Max = total;
            _master.Value = current;
            Paint(_master);
        }

        public ProgressLogger CreateProgress(string title, long max = 0)
        {
            _master ??= new ProgressLogger(this, true);
            _master.Title = title;
            _master.Max = max;
            _master.Value = 0;
            Paint(_master);
            return _master;
        }

        public ProgressLogger CreateSubProgress(string title, long max = 0)
        {
            _child ??= new ProgressLogger(this, false);
            _child.Title = title;
            _child.Max = max;
            _child.Value = 0;
            Paint(_child);
            return _child;
        }

        public static char GetSpinner(int counter)
        {
            return _spinner[counter % _spinner.Length];
        }

        private void SetColor(LogLevel level)
        {
            Console.ForegroundColor = level switch
            {
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Warn => ConsoleColor.Yellow,
                _ => _defaultColor
            };
        }

        private void Paint(ProgressLogger progress)
        {
            lock (Console.Out)
            {
                int originalLeft = Console.CursorLeft;
                int originalTop = Console.CursorTop;
                if (!progress.IsMaster && _master is not null)
                {
                    PaintProgress(_master);
                }
                PaintProgress(progress);
                Console.SetCursorPosition(originalLeft, originalTop);
            }
        }

        private void PaintProgress(ProgressLogger progress)
        {
            Console.SetCursorPosition(0, progress.IsMaster ? 0 : 1);
            var text = progress.Title.Trim('.');
            if (progress.Max == 0)
            {
                text = $"{text}: [{GetSpinner(_counter++)}] {progress.Value}...";
            } else
            {
                var val = (int)((double)progress / 5);
                text = $"{text}: [{new string('=', val)}{new string(' ', 20 - val)}] {progress.Value}/{progress.Max}";
            }
            Console.Write(text);
            if (text.Length < Console.BufferWidth)
            {
                Console.Write(new string(' ', Console.BufferWidth - text.Length));
            }
        }

        public void Flush()
        {
        }

        public void Dispose()
        {
        }
    }
}
