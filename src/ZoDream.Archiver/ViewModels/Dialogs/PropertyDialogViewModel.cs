using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class PropertyDialogViewModel: BindableBase
    {

        private string? _typeName;

        public string? TypeName {
            get => _typeName;
            set => Set(ref _typeName, value);
        }


        private string? _version;

        public string? Version {
            get => _version;
            set => Set(ref _version, value);
        }

        private bool _isEncrypted;

        public bool IsEncrypted {
            get => _isEncrypted;
            set => Set(ref _isEncrypted, value);
        }

        private long _length;

        public long Length {
            get => _length;
            set => Set(ref _length, value);
        }


    }
}
