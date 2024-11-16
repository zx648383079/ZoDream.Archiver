using ZoDream.Shared.Bundle;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class PropertyDialogViewModel: BindableBase, IBundleOptions
    {
        public bool LeaveStreamOpen { get; set; }
        public bool LookForHeader { get; set; }

        private string? _password;
        public string? Password {
            get => _password;
            set => Set(ref _password, value);
        }
        private string? _dictionary;

        public string? Dictionary {
            get => _dictionary;
            set => Set(ref _dictionary, value);
        }


        private string? _engine;

        public string? Engine {
            get => _engine;
            set => Set(ref _engine, value);
        }

        private string? _platform;

        public string? Platform {
            get => _platform;
            set => Set(ref _platform, value);
        }

        private string? _package;

        public string? Package {
            get => _package;
            set => Set(ref _package, value);
        }

        private string? _displayName;

        public string? DisplayName {
            get => _displayName;
            set => Set(ref _displayName, value);
        }


        private string? _producer;

        public string? Producer {
            get => _producer;
            set => Set(ref _producer, value);
        }

        private string? _version;

        public string? Version {
            get => _version;
            set => Set(ref _version, value);
        }

        private string? _entrance;

        public string? Entrance {
            get => _entrance;
            set => Set(ref _entrance, value);
        }

        public void Load(IBundleOptions? options)
        {
            if (options is null)
            {
                return;
            }
            Password = options.Password;
            Package = options.Package;
            Producer = options.Producer;
            Version = options.Version;
            Engine = options.Engine;
            Platform = options.Platform;
            Entrance = options.Entrance;
            Dictionary = options.Dictionary;
            DisplayName = options.DisplayName;
        }

    }
}
