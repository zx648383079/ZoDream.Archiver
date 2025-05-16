using System;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.Models;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class RequestDialogViewModel: BindableBase, IFormValidator
    {
        public RequestDialogViewModel()
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

        public ArchiveExtractMode ExtractMode {
            get => (ArchiveExtractMode)(TypeIndex + 1);
            set => TypeIndex = (int)value - 1;
        }

        private string _link = string.Empty;

        public string Link {
            get => _link;
            set {
                Set(ref _link, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }


        private string _outputFolder = string.Empty;

        public string OutputFolder {
            get => _outputFolder;
            set {
                Set(ref _outputFolder, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private string _password = string.Empty;

        public string Password {
            get => _password;
            set => Set(ref _password, value);
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

        public bool IsValid => !string.IsNullOrWhiteSpace(OutputFolder) && !string.IsNullOrWhiteSpace(Link);

        public ICommand OpenCommand { get; private set; }

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
            OutputFolder = folder.Path;
        }
    }
}
