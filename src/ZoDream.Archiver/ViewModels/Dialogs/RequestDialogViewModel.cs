using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.BundleExtractor.Engines;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Net;

namespace ZoDream.Archiver.ViewModels
{
    public class RequestDialogViewModel: ObservableObject, IFormValidator
    {
        public RequestDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
        }

        private string[] _typeItems = ["覆盖", "跳过", "重命名"];

        public string[] TypeItems {
            get => _typeItems;
            set => SetProperty(ref _typeItems, value);
        }

        private int _typeIndex;

        public int TypeIndex {
            get => _typeIndex;
            set => SetProperty(ref _typeIndex, value);
        }

        private string[] _engineItems = ["Auto", "Egret"];

        public string[] EngineItems {
            get => _engineItems;
            set => SetProperty(ref _engineItems, value);
        }

        private int _engineIndex;

        public int EngineIndex {
            get => _engineIndex;
            set => SetProperty(ref _engineIndex, value);
        }

        public ArchiveExtractMode ExtractMode {
            get => (ArchiveExtractMode)(TypeIndex + 1);
            set => TypeIndex = (int)value - 1;
        }

        private string _link = string.Empty;

        public string Link {
            get => _link;
            set {
                SetProperty(ref _link, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }


        private string _outputFolder = string.Empty;

        public string OutputFolder {
            get => _outputFolder;
            set {
                SetProperty(ref _outputFolder, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private string _password = string.Empty;

        public string Password {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _enabledImage;

        public bool EnabledImage {
            get => _enabledImage;
            set => SetProperty(ref _enabledImage, value);
        }

        private bool _enabledVideo;

        public bool EnabledVideo {
            get => _enabledVideo;
            set => SetProperty(ref _enabledVideo, value);
        }

        private bool _enabledAudio;

        public bool EnabledAudio {
            get => _enabledAudio;
            set => SetProperty(ref _enabledAudio, value);
        }

        private bool _enabledShader;

        public bool EnabledShader {
            get => _enabledShader;
            set => SetProperty(ref _enabledShader, value);
        }

        private bool _enabledLua;

        public bool EnabledLua {
            get => _enabledLua;
            set => SetProperty(ref _enabledLua, value);
        }

        private bool _enabledJson;

        public bool EnabledJson {
            get => _enabledJson;
            set => SetProperty(ref _enabledJson, value);
        }


        private bool _enabledSpine;

        public bool EnabledSpine {
            get => _enabledSpine;
            set => SetProperty(ref _enabledSpine, value);
        }

        private bool _enabledModel;

        public bool EnabledModel {
            get => _enabledModel;
            set => SetProperty(ref _enabledModel, value);
        }

        public string[] ModelFormatItems { get; private set; } = ["gltf", "glb", "fbx"];

        private string _modelFormat = "gltf";

        public string ModelFormat {
            get => _modelFormat;
            set => SetProperty(ref _modelFormat, value);
        }

        private int _maxBatchCount = 100;

        public int MaxBatchCount {
            get => _maxBatchCount;
            set => SetProperty(ref _maxBatchCount, value);
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(OutputFolder) && !string.IsNullOrWhiteSpace(Link);

        public ICommand OpenCommand { get; private set; }

        public IBundleExecutor Executor => EngineIndex switch
        {
            1 => new EgretEngine(),
            _ => new NetExecutor(),
        };

        private async void TapOpen()
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
