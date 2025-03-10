using System;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Archiver.Dialogs;

namespace ZoDream.Archiver.ViewModels
{
    internal partial class AppViewModel
    {

        private ProgressDialog? _progress;
        private CancellationTokenSource? _progressToken;

        public CancellationToken OpenProgress(string title = "压缩中...")
        {
            _progressToken = new CancellationTokenSource();
            _ = CreateProgressAsync(title);
            return _progressToken.Token;
        }

        public void CloseProgress()
        {
            if (_progress is null)
            {
                return;
            }
            DispatcherQueue.TryEnqueue(() => {
                _progress.Hide();
                _progress.ViewModel.Dispose();
                _progress = null;
                _progressToken?.Dispose();
                _progressToken = null;
            });
        }

        private async Task CreateProgressAsync(string title)
        {
            _progress = new ProgressDialog
            {
                Title = title
            };
            await OpenDialogAsync(_progress);
            _progressToken?.Cancel();
        }
        public void UpdateProgress(double progress)
        {
            UpdateProgress(progress, null);
        }
        public void UpdateProgress(double progress, string? message) 
        {
            if (_progress is null)
            {
                return;
            }
            DispatcherQueue.TryEnqueue(() => {
                if (progress >= 0)
                {
                    _progress.ViewModel.Progress = progress * 100;
                }
                if (message is not null)
                {
                    _progress.ViewModel.Message = message;
                }
            });
        }
    }
}
