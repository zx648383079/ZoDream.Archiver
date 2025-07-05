using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoDream.Archiver.ViewModels
{
    public class PropertyDialogViewModel: ObservableObject
    {

        private string? _typeName;

        public string? TypeName {
            get => _typeName;
            set => SetProperty(ref _typeName, value);
        }


        private string? _version;

        public string? Version {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        private bool _isEncrypted;

        public bool IsEncrypted {
            get => _isEncrypted;
            set => SetProperty(ref _isEncrypted, value);
        }

        private long _length;

        public long Length {
            get => _length;
            set => SetProperty(ref _length, value);
        }


    }
}
