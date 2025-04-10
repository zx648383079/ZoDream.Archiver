﻿using Microsoft.UI.Xaml;
using System;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.Models;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class ExtractDialogViewModel : BindableBase, IFormValidator
    {
        public ExtractDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
            DictCommand = new RelayCommand(TapDict);
        }

        private string[] _typeItems = ["覆盖", "跳过", "重命名"];

        public string[] TypeItems {
            get => _typeItems;
            set => Set(ref _typeItems, value);
        }

        private int _typeIndex;

        public int TypeIndex {
            get => _typeIndex;
            set {
                Set(ref _typeIndex, value);
            }
        }

        public ArchiveExtractMode ExtractMode => (ArchiveExtractMode)(TypeIndex + 1);

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
            set {
                Set(ref _password, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private string _dictFileName = string.Empty;

        public string DictFileName {
            get => _dictFileName;
            set {
                Set(ref _dictFileName, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private bool _onlySelected;

        public bool OnlySelected {
            get => _onlySelected;
            set => Set(ref _onlySelected, value);
        }

        private Visibility _isSelected = Visibility.Collapsed;

        public Visibility IsSelected {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        private Visibility _isEncrypted = Visibility.Collapsed;

        public Visibility IsEncrypted {
            get => _isEncrypted;
            set => Set(ref _isEncrypted, value);
        }

        public bool IsValid {
            get {
                if (IsEncrypted == Visibility.Visible)
                {
                    if (string.IsNullOrWhiteSpace(DictFileName) || string.IsNullOrWhiteSpace(Password))
                    {
                        return false;
                    }
                }
                if (string.IsNullOrWhiteSpace(FileName))
                {
                    return false;
                }
                return true;
            }
        }
        public ICommand OpenCommand { get; private set; }
        public ICommand DictCommand { get; private set; }

        private async void TapOpen(object? _)
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            App.ViewModel.InitializePicker(picker);
            // picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            FileName = folder.Path;
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

    }
}
