using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Archiver.Converters;
using ZoDream.Shared.Compression.Own;
using ZoDream.Shared.Compression.Tar;
using ZoDream.Shared.Compression.Zip;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.Archiver.ViewModels
{
    public class CompressDialogViewModel: ObservableObject, IFormValidator
    {
        public CompressDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
            DictCommand = new RelayCommand(TapDict);
        }

        public string[] TypeItems => ["zip", "tar", "bz", "特殊", "字典"];

        private int _typeIndex;

        public int TypeIndex {
            get => _typeIndex;
            set {
                SetProperty(ref _typeIndex, value);
                DictVisible = value == 3 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string[] VersionItems => ["默认", "v1", "v2", "v3"];

        private int _versionIndex;

        public int VersionIndex {
            get => _versionIndex;
            set => SetProperty(ref _versionIndex, value);
        }

        private string[] _subVolumeItems = ["100MB", "500MB", "1GB", "4GB", "8GB"];

        public string[] SubVolumeItems {
            get => _subVolumeItems;
            set => SetProperty(ref _subVolumeItems, value);
        }


        private string _subVolumeText = string.Empty;

        public string SubVolumeText {
            get => _subVolumeText;
            set => SetProperty(ref _subVolumeText, value);
        }


        private string _fileName = string.Empty;

        public string FileName {
            get => _fileName;
            set {
                SetProperty(ref _fileName, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }


        private string _password = string.Empty;

        public string Password {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _dictFileName = string.Empty;

        public string DictFileName {
            get => _dictFileName;
            set => SetProperty(ref _dictFileName, value);
        }

        private Visibility _dictVisible = Visibility.Collapsed;

        public Visibility DictVisible {
            get => _dictVisible;
            set => SetProperty(ref _dictVisible, value);
        }


        public long SubVolumeLength => SizeConverter.Parse(SubVolumeText, "M");

        public bool IsValid => !string.IsNullOrWhiteSpace(FileName);

        public ICommand OpenCommand { get; private set; }
        public ICommand DictCommand { get; private set; }
        private async void TapOpen()
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("压缩文件", [ ".zip", ".tar", 
                ".gz", ".bz2", ".z"]);
            picker.FileTypeChoices.Add("字典", [ ".bin" ]);
            //picker.FileTypeChoices.Add("所有文件", [ "*" ]);
            App.ViewModel.InitializePicker(picker);
            var res = await picker.PickSaveFileAsync();
            if (res is null)
            {
                return;
            }
            FileName = res.Path;
        }
        private async void TapDict()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");
            App.ViewModel.InitializePicker(picker);
            var res = await picker.PickSingleFileAsync();
            if (res is null)
            {
                return;
            }
            DictFileName = res.Path;
        }

        public IArchiveWriter? Create()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                return null;
            }
            var option = new ArchiveOptions(Password, DictFileName)
            {
                LeaveStreamOpen = false,
            };
            var subVolume = SubVolumeLength;
            Stream fs = subVolume > 0 ? new MultipartOutputStream(FileName, subVolume) :
                File.Create(FileName);
            return TypeIndex switch
            {
                4 => new OwnDictionaryWriter(fs, option),
                3 => new OwnArchiveWriter(fs, (OwnVersion)VersionIndex, option),
                2 or 1 => new TarArchiveWriter(fs, option),
                _ => new ZipArchiveWriter(fs, option),
            };
        }
    }
}
