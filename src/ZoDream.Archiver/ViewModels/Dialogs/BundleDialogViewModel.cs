using Microsoft.UI.Xaml;
using System;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.BundleExtractor;
using ZoDream.Shared.Bundle;
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

        private string[] _engineItems = ["Auto",];

        public string[] EngineItems {
            get => _engineItems;
            set => Set(ref _engineItems, value);
        }

        private int _engineIndex;

        public int EngineIndex {
            get => _engineIndex;
            set => Set(ref _engineIndex, value);
        }

        private string[] _platformItems = [
            "Auto",
        ];

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

        private static int IndexOf(string[] items, string? item)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return 0;
            }
            for (var i = 1; i < items.Length; i++)
            {
                if (items[i] == item)
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="options"></param>
        public void Load(BundleScheme scheme, IBundleOptions? options)
        {
            if (PlatformItems.Length <= 1)
            {
                PlatformItems = ["Auto", .. scheme.PlatformNames];
                EngineItems = ["Auto", .. scheme.EngineNames];
            }
            if (options is not null)
            {
                Password = options.Password ?? string.Empty;
                ApplicationId = options.Package ?? string.Empty;
                PlatformIndex = IndexOf(PlatformItems, options.Platform);
                EngineIndex = IndexOf(EngineItems, options.Engine);
            }
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <param name="options"></param>
        public void Unload(IBundleOptions options)
        {
            if (options is BundleOptions o)
            {
                o.Password = Password;
            }
            if (!string.IsNullOrWhiteSpace(ApplicationId))
            {
                options.Package = ApplicationId;
            }
            if (PlatformIndex > 0)
            {
                options.Platform = PlatformItems[PlatformIndex];
            }
            if (EngineIndex > 0)
            {
                options.Engine = EngineItems[EngineIndex];
            }
        }
    }
}
