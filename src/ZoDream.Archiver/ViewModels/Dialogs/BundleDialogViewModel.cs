using Microsoft.UI.Xaml;
using System;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.Models;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class BundleDialogViewModel: BindableBase
    {
        public BundleDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
        }

        private string[] _typeItems = ["覆盖", "跳过", "重命名"];

        public string[] TypeItems {
            get => _typeItems;
            set => Set(ref _typeItems, value);
        }

        private int _typeIndex;

        public int TypeIndex {
            get => _typeIndex;
            set => Set(ref _typeIndex, value);
        }

        private string[] _engineItems = ["Auto", "Unity", "Unreal", "Godot", "Cocos"];

        public string[] EngineItems {
            get => _engineItems;
            set => Set(ref _engineItems, value);
        }

        private int _engineIndex;

        public int EngineIndex {
            get => _engineIndex;
            set => Set(ref _engineIndex, value);
        }

        private string[] _platformItems = ["Auto", "Android", "IOS", "Windows"];

        public string[] PlatformItems {
            get => _platformItems;
            set => Set(ref _platformItems, value);
        }

        private int _platformIndex;

        public int PlatformIndex {
            get => _platformIndex;
            set => Set(ref _platformIndex, value);
        }

        private string _applicationId = string.Empty;

        public string ApplicationId {
            get => _applicationId;
            set => Set(ref _applicationId, value);
        }

        public ArchiveExtractMode ExtractMode => (ArchiveExtractMode)(TypeIndex + 1);

        private string _fileName = string.Empty;

        public string FileName {
            get => _fileName;
            set => Set(ref _fileName, value);
        }

        private string _password = string.Empty;

        public string Password {
            get => _password;
            set => Set(ref _password, value);
        }

        private Visibility _isEncrypted = Visibility.Collapsed;

        public Visibility IsEncrypted {
            get => _isEncrypted;
            set => Set(ref _isEncrypted, value);
        }


        public ICommand OpenCommand { get; private set; }

        private async void TapOpen(object? _)
        {
            var picker = new FolderPicker();
            App.ViewModel.InitializePicker(picker);
            // picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            FileName = folder.Path;
        }


        public bool Verify()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                return false;
            }
            return true;
        }
    }
}
