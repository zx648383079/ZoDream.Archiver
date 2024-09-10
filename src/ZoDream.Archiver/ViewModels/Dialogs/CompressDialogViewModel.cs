using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.Compression.Own;
using ZoDream.Shared.Compression.Tar;
using ZoDream.Shared.Compression.Zip;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class CompressDialogViewModel: BindableBase
    {
        public CompressDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
            DictCommand = new RelayCommand(TapDict);
        }

        private string[] _typeItems = ["zip", "tar", "bz", "特殊", "字典"];

        public string[] TypeItems {
            get => _typeItems;
            set => Set(ref _typeItems, value);
        }

        private int _typeIndex;

        public int TypeIndex {
            get => _typeIndex;
            set {
                Set(ref _typeIndex, value);
                DictVisible = value == 3 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

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

        private string _dictFileName = string.Empty;

        public string DictFileName {
            get => _dictFileName;
            set => Set(ref _dictFileName, value);
        }

        private Visibility _dictVisible = Visibility.Collapsed;

        public Visibility DictVisible {
            get => _dictVisible;
            set => Set(ref _dictVisible, value);
        }

        public ICommand OpenCommand { get; private set; }
        public ICommand DictCommand { get; private set; }
        private async void TapOpen(object? _)
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
        private async void TapDict(object? _)
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
            var fs = File.Create(FileName);
            return TypeIndex switch
            {
                4 => new OwnDictionaryWriter(fs, option),
                3 => new OwnArchiveWriter(fs, option),
                2 or 1 => new TarArchiveWriter(fs, option),
                _ => new ZipArchiveWriter(fs, option),
            };
        }
    }
}
