using Microsoft.UI.Xaml;
using System;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.BundleExtractor;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class BundleDialogViewModel: BindableBase, IFormValidator, IEntryConfiguration
    {
        public BundleDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
            FolderCommand = new RelayCommand(TapFolder);
            OpenDependencyCommand = new RelayCommand(TapOpenDependency);
            CreateDependencyCommand = new RelayCommand(TapCreateDependency);
        }


        private bool _isCreateDependencyTask = false;

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

        private string _entrance = string.Empty;

        public string Entrance {
            get => _entrance;
            set => Set(ref _entrance, value);
        }

        public ArchiveExtractMode ExtractMode {
            get => (ArchiveExtractMode)(TypeIndex + 1);
            set => TypeIndex = (int)value - 1;
        }

        private string _fileName = string.Empty;

        public string FileName {
            get => _fileName;
            set {
                Set(ref _fileName, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private string _password = string.Empty;

        public string Password {
            get => _password;
            set => Set(ref _password, value);
        }

        private string _dependencySource = string.Empty;

        public string DependencySource {
            get => _dependencySource;
            set => Set(ref _dependencySource, value);
        }

        private bool _enabledImage;

        public bool EnabledImage {
            get => _enabledImage;
            set => Set(ref _enabledImage, value);
        }

        private bool _enabledVideo;

        public bool EnabledVideo {
            get => _enabledVideo;
            set => Set(ref _enabledVideo, value);
        }

        private bool _enabledAudio;

        public bool EnabledAudio {
            get => _enabledAudio;
            set => Set(ref _enabledAudio, value);
        }

        private bool _enabledShader;

        public bool EnabledShader {
            get => _enabledShader;
            set => Set(ref _enabledShader, value);
        }

        private bool _enabledLua;

        public bool EnabledLua {
            get => _enabledLua;
            set => Set(ref _enabledLua, value);
        }

        private bool _enabledJson;

        public bool EnabledJson {
            get => _enabledJson;
            set => Set(ref _enabledJson, value);
        }


        private bool _enabledSpine;

        public bool EnabledSpine {
            get => _enabledSpine;
            set => Set(ref _enabledSpine, value);
        }

        private bool _enabledModel;

        public bool EnabledModel {
            get => _enabledModel;
            set => Set(ref _enabledModel, value);
        }

        public string[] ModelFormatItems { get; private set; } = ["gltf", "glb", "fbx"];

        private string _modelFormat = "gltf";

        public string ModelFormat {
            get => _modelFormat;
            set => Set(ref _modelFormat, value);
        }

        private int _maxBatchCount = 100;

        public int MaxBatchCount {
            get => _maxBatchCount;
            set => Set(ref _maxBatchCount, value);
        }


        private Visibility _isEncrypted = Visibility.Collapsed;

        public Visibility IsEncrypted {
            get => _isEncrypted;
            set => Set(ref _isEncrypted, value);
        }


        public bool IsValid => !string.IsNullOrWhiteSpace(FileName);

        public ICommand OpenCommand { get; private set; }
        public ICommand OpenDependencyCommand { get; private set; }
        public ICommand CreateDependencyCommand { get; private set; }
        public ICommand FolderCommand { get; private set; }
        private async void TapCreateDependency(object? _)
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("依赖文件", [".bin"]);
            picker.SuggestedFileName = "dependencies";
            App.ViewModel.InitializePicker(picker);
            var res = await picker.PickSaveFileAsync();
            if (res is null)
            {
                return;
            }
            DependencySource = res.Path;
            _isCreateDependencyTask = true;
        }
        private async void TapOpenDependency(object? _)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".bin");
            picker.FileTypeFilter.Add(".json");
            App.ViewModel.InitializePicker(picker);
            var res = await picker.PickSingleFileAsync();
            if (res is null)
            {
                return;
            }
            DependencySource = res.Path;
            _isCreateDependencyTask = false;
        }

        private async void TapFolder(object? _)
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            App.ViewModel.InitializePicker(picker);
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            Entrance = folder.Path;
        }

        private async void TapOpen(object? _)
        {
            var picker = new FolderPicker();
            App.ViewModel.InitializePicker(picker);
            picker.FileTypeFilter.Add("*");
            // picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            FileName = folder.Path;
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
                Entrance = options.Entrance ?? string.Empty;
            }
            if (options is IArchiveExtractOptions o)
            {
                FileName = o.OutputFolder;
                ExtractMode = o.FileMode;
            }
            if (options is IBundleExtractOptions b)
            {
                DependencySource = b.DependencySource;
                EnabledAudio = b.EnabledAudio;
                EnabledImage = b.EnabledImage;
                EnabledLua = b.EnabledLua;
                EnabledModel = b.EnabledModel;
                EnabledShader = b.EnabledShader;
                EnabledSpine = b.EnabledSpine;
                EnabledVideo = b.EnabledVideo;
                EnabledJson = b.EnabledJson;
                ModelFormat = b.ModelFormat;
                MaxBatchCount = b.MaxBatchCount;
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
                o.OutputFolder = FileName;
                o.FileMode = ExtractMode;
                o.EnabledVideo = EnabledAudio;
                o.EnabledAudio = EnabledImage;
                o.EnabledLua = EnabledLua;
                o.EnabledModel = EnabledModel;
                o.EnabledShader = EnabledShader;
                o.ModelFormat = ModelFormat;
                o.EnabledImage = EnabledImage;
                o.EnabledSpine = EnabledSpine;
                o.EnabledJson = EnabledJson;
                o.DependencySource = DependencySource;
                o.MaxBatchCount = MaxBatchCount;
                o.OnlyDependencyTask = _isCreateDependencyTask;
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
            if (!string.IsNullOrWhiteSpace(Entrance))
            {
                options.Entrance = Entrance;
            }
        }

        public void Load(IEntryService service, object options)
        {
            service.AddIf<BundleScheme>();
            Load(service.Get<BundleScheme>(), options as IBundleOptions);
        }

        public void Unload(IEntryService service, object options)
        {
            if (options is IBundleOptions o)
            {
                Unload(o);
            } 
        }
    }
}
