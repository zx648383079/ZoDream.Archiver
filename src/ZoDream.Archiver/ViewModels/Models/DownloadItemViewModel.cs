using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Net.Http;
using System.Windows.Input;
using ZoDream.Archiver.Controls;
using ZoDream.Archiver.Converters;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Net;

namespace ZoDream.Archiver.ViewModels
{
    public class DownloadItemViewModel : ObservableObject, IEntryItem
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
        private readonly BundleTokenSource _tokenSource = new();
        private readonly Bandwidth _bandwidth = new();
        public Uri Source { get; private set; }
        public string Target { get; set; } = string.Empty;

        public string Icon => IconConverter.Format(this);

        private string _name = string.Empty;

        public string Name {
            get => _name;
            set {
                SetProperty(ref _name, value);
                OnPropertyChanged(nameof(Icon));
            }
        }

        private BundleStatus _status;

        public BundleStatus Status {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private long _length;

        public long Length {
            get => _length;
            set => SetProperty(ref _length, value);
        }

        private long _value;

        public long Value {
            get => _value;
            set => SetProperty(ref _value, value);
        }


        public long Speed => (long)_bandwidth.Speed;

        public int ElapsedTime => Speed > 0 ? (int)((Length - Value) / Speed) : 0;

        public bool IsDirectory => false;

        public string Extension => string.IsNullOrWhiteSpace(Name) ? string.Empty : Path.GetExtension(Name).ToLower();

        private DateTime _lastModified = DateTime.MinValue;

        public DateTime LastModified {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }

        private double _progress;

        public double Progress {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }


        private string _message = string.Empty;

        public string Message {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        

        public ICommand PlayCommand { get; private set; }
        public ICommand ResumeCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        private void TapPlay()
        {
            _host.PlayCommand.Execute(this);
        }
        private void TapResume()
        {
            if (Status == BundleStatus.Paused)
            {
                _tokenSource.Resume();
                return;
            }
            if (Status is BundleStatus.Sending or BundleStatus.Receiving)
            {
                return;
            }
            _tokenSource.Cancel();
            _host.PlayCommand.Execute(this);
        }
        private void TapPause()
        {
            _tokenSource.Pause();
            Status = BundleStatus.Paused;
        }
        private void TapStop()
        {
            _tokenSource.Cancel();
            Status = BundleStatus.Cancelled;
        }
        private void TapDelete()
        {
            _host.DeleteCommand.Execute(this);
        }

        public IBundleRequest CreateRequest()
        {
            return new RequestContext()
            {
                Method = HttpMethod.Get,
                Source = Source,
                Token = _tokenSource.Token,
                RequestId = GetHashCode(),
                Output = Target,
                SuggestedName = Name,
            };
        }

        private void Token_RequestChanged(BundleChangedEventArgs args)
        {
            App.ViewModel.DispatcherQueue.TryEnqueue(() => {
                if (!string.IsNullOrEmpty(args.Name))
                {
                    Name = args.Name;
                }
                if (args.Length != 0)
                {
                    Length = args.Length;
                    Progress = 0;
                }
                Status = args.Status;
                LastModified = DateTime.Now;
                Message = ConverterHelper.FormatSpeed(this);
            });
        }

        private void Token_ProgressChanged(long received)
        {
            App.ViewModel.DispatcherQueue.TryEnqueue(() => {
                Value = received;
                _bandwidth.CalculateSpeed(Value);
                Progress = Length > 0 ? (double)Value * 100 / Length : 0;
                Message = ConverterHelper.FormatSpeed(this);
            });
            
        }
    }
}
