using System;
using System.IO;
using System.Net.Http;
using System.Windows.Input;
using ZoDream.Archiver.Controls;
using ZoDream.Archiver.Converters;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Net;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class DownloadItemViewModel : BindableBase, IEntryItem
    {

        public DownloadItemViewModel(DownloadViewModel host, Uri source)
        {
            _host = host;
            Source = source;
            PlayCommand = UICommand.Play(TapPlay);
            ResumeCommand = UICommand.Resume(TapResume);
            PauseCommand = UICommand.Pause(TapPause);
            StopCommand = UICommand.Stop(TapStop);
            DeleteCommand = UICommand.Delete(TapDelete);
            _tokenSource.ProgressChanged += Token_ProgressChanged;
            _tokenSource.RequestChanged += Token_RequestChanged;
        }

        private readonly DownloadViewModel _host;
        private readonly RequestTokenSource _tokenSource = new();
        public Uri Source { get; private set; }
        public string Target { get; set; } = string.Empty;

        public string Icon => IconConverter.Format(this);

        private string _name = string.Empty;

        public string Name {
            get => _name;
            set {
                Set(ref _name, value);
                OnPropertyChanged(nameof(Icon));
            }
        }

        private RequestStatus _status;

        public RequestStatus Status {
            get => _status;
            set => Set(ref _status, value);
        }

        private long _length;

        public long Length {
            get => _length;
            set => Set(ref _length, value);
        }

        private long _value;

        public long Value {
            get => _value;
            set => Set(ref _value, value);
        }

        private long _speed;

        public long Speed {
            get => _speed;
            set => Set(ref _speed, value);
        }

        public int ElapsedTime => Speed > 0 ? (int)((Length - Value) / Speed) : 0;

        public bool IsDirectory => false;

        public string Extension => string.IsNullOrWhiteSpace(Name) ? string.Empty : Path.GetExtension(Name).ToLower();

        public ICommand PlayCommand { get; private set; }
        public ICommand ResumeCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        private void TapPlay(object? _)
        {
            _host.PlayCommand.Execute(this);
        }
        private void TapResume(object? _)
        {
            _tokenSource.Resume();
        }
        private void TapPause(object? _)
        {
            _tokenSource.Pause();
        }
        private void TapStop(object? _)
        {
            _tokenSource.Cancel();
        }
        private void TapDelete(object? _)
        {
            _host.DeleteCommand.Execute(this);
        }

        public RequestContext CreateRequest()
        {
            return new RequestContext()
            {
                Method = HttpMethod.Get,
                Source = Source,
                Token = _tokenSource.Token,
                SourceId = GetHashCode(),
                OutputFolder = Target
            };
        }

        private void Token_RequestChanged(RequestChangedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.FileName))
            {
                Name = args.FileName;
            }
            if (args.Length != 0)
            {
                Length = args.Length;
            }
            Status = args.Status;
        }

        private void Token_ProgressChanged(long received)
        {
            Value = received;
        }
    }
}
