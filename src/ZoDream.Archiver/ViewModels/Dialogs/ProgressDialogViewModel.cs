using System;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Logging;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class ProgressDialogViewModel: BindableBase, IDisposable
    {
        public ProgressDialogViewModel()
        {
            if (_app.Logger is EventLogger logger)
            {
                logger.OnLog += Logger_OnLog;
                logger.OnProgress += Logger_OnProgress;
            }
        }

        private void Logger_OnProgress(long current, long total, string message)
        {
            _messageRefreshToken.Cancel();
            _app.DispatcherQueue.TryEnqueue(() => {
                Message = message;
                Progress = total > 0 ? (current * 100 / total) : 0;
            });
        }

        private void Logger_OnLog(string message, Shared.Models.LogLevel level)
        {
            _messageRefreshToken.Cancel();
            if (level == Shared.Models.LogLevel.Info)
            {
                _lastInfoMessage = message;
            }
            _app.DispatcherQueue.TryEnqueue(() => {
                Message = message;
            });
            // 错误信息只允许显示一次
            if (level == Shared.Models.LogLevel.Info)
            {
                return;
            }
            _messageRefreshToken = new();
            var token = _messageRefreshToken.Token;
            Task.Factory.StartNew(() => {
                Thread.Sleep(10000);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                _app.DispatcherQueue.TryEnqueue(() => {
                    Message = _lastInfoMessage;
                });
            }, token);
        }

        private readonly AppViewModel _app = App.ViewModel;
        private readonly DateTime _beginTime = DateTime.Now;
        private CancellationTokenSource _messageRefreshToken = new();
        private string _lastInfoMessage = string.Empty;

        private int _elapsedTime;

        public int ElapsedTime {
            get => _elapsedTime;
            set => Set(ref _elapsedTime, value);
        }

        private int _timeLeft;

        public int TimeLeft {
            get => _timeLeft;
            set => Set(ref _timeLeft, value);
        }

        private bool _progressUnknow = true;

        public bool ProgressUnknow {
            get => _progressUnknow;
            set => Set(ref _progressUnknow, value);
        }


        private double _progress;
        /// <summary>
        /// %
        /// </summary>
        public double Progress {
            get => _progress;
            set {
                Set(ref _progress, value);
                if (value > 0)
                {
                    ProgressUnknow = false;
                    Computed();
                }
            }
        }

        private string _message = string.Empty;

        public string Message {
            get => _message;
            set {
                Set(ref _message, value);
                if (Progress > 0)
                {
                    Computed();
                }
            }
        }

        private void Computed()
        {
            if (ProgressUnknow)
            {
                return;
            }
            var diff = DateTime.Now - _beginTime;
            ElapsedTime = (int)diff.TotalSeconds;
            TimeLeft = (int)(diff.TotalSeconds * 100 / Progress - diff.TotalSeconds);
        }

        public void Dispose()
        {
            if (_app.Logger is EventLogger logger)
            {
                logger.OnLog -= Logger_OnLog;
                logger.OnProgress -= Logger_OnProgress;
            }
        }
    }
}
