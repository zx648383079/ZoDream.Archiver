using Microsoft.UI.Xaml.Input;
using System.IO;
using System.Windows.Input;
using ZoDream.Archiver.Converters;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Net;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class DownloadItemViewModel : BindableBase, IEntryItem
    {
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

        private int _timeLeft;

        public int TimeLeft {
            get => _timeLeft;
            set => Set(ref _timeLeft, value);
        }


        public bool IsDirectory => false;

        public string Extension => string.IsNullOrWhiteSpace(Name) ? string.Empty : Path.GetExtension(Name).ToLower();
        public ICommand DeleteCommand { get; private set; }
        public DownloadItemViewModel()
        {
            DeleteCommand = new StandardUICommand(StandardUICommandKind.Delete)
            {
                Command = new RelayCommand(TapDelete)
            };
        }

        private void TapDelete(object? _)
        {

        }
    }
}
