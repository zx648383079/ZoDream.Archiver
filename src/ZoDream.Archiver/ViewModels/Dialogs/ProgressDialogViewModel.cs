using System;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;
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
            StartTick();
        }

        private void Logger_OnProgress(long current, long total, string message)
        {
            _messageRefreshToken.Cancel();
            _app.DispatcherQueue.TryEnqueue(() => {
                Message = message;
                if (current > 0 || total > 0)
                {
                    ProgressUnknow = false;
                }
                Progress = total > 0 ? ((double)current * 100 / total) : 0;
            });
        }

        private void Logger_OnLog(string message, LogLevel level)
        {
            if (level == LogLevel.Warn)
            {
                return;
            }
            _messageRefreshToken.Cancel();
            if (level == LogLevel.Info)
            {
                _lastInfoMessage = message;
            }
            _app.DispatcherQueue.TryEnqueue(() => {
                Message = message;
            });
            // 错误信息只允许显示一次
            if (level == LogLevel.Info)
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
        private CancellationTokenSource _timeToken = new();
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
                if (Set(ref _progress, value) && (value > 0 || !ProgressUnknow))
                {
                    ProgressUnknow = false;
                    Computed(true);
                }
            }
        }

        private string _message = string.Empty;

        public string Message {
            get => _message;
            set {
                Set(ref _message, value);
            }
        }

        /// <summary>
        /// 只有进度更新了才更新剩余时间
        /// </summary>
        /// <param name="fromProgress"></param>
        private void Computed(bool fromProgress = false)
        {
            var diff = DateTime.Now - _beginTime;
            var lastDiff = ElapsedTime;
            ElapsedTime = (int)diff.TotalSeconds;
            if (ProgressUnknow)
            {
                return;
            }
            if (fromProgress)
            {
                TimeLeft = Progress > 0 ? (int)(diff.TotalSeconds * 100 / Progress - diff.TotalSeconds) : 0;
            } 
            else if (TimeLeft > 0)
            {
                TimeLeft -= Math.Max(ElapsedTime - lastDiff, 0);
            }
        }

        private void StartTick()
        {
            var token = _timeToken.Token;
            Task.Factory.StartNew(() => {
                while (!token.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                    _app.DispatcherQueue.TryEnqueue(() => {
                        Computed();
                    });
                }
            }, token);
        }

        public void Dispose()
        {
            if (_app.Logger is EventLogger logger)
            {
                logger.OnLog -= Logger_OnLog;
                logger.OnProgress -= Logger_OnProgress;
            }
            _timeToken.Cancel();
        }
    }
}
