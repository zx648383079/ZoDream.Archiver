﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class SettingDialogViewModel : ObservableObject
    {

        public SettingDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
            OpenTemporaryCommand = new RelayCommand(TapOpenTemporary);
            ClearTemporaryCommand = new RelayCommand(TapClearTemporary);
            _setting = App.ViewModel.Setting;
            FFmpegBin = _setting.Get<string>(SettingNames.FFmpegPath);
            TemporaryFolder = _setting.Get<string>(SettingNames.TemporaryPath);
            ModelFormat = _setting.Get<string>(SettingNames.ModelFormat);
            MaxBatchCount = _setting.Get<int>(SettingNames.MaxBatchCount);
        }

        private readonly ISettingContainer _setting;

        public string[] ModelFormatItems { get; private set; } = ["gltf", "glb", "fbx"];


        private string _ffmpegBin = string.Empty;

        public string FFmpegBin {
            get => _ffmpegBin;
            set => SetProperty(ref _ffmpegBin, value);
        }

        private string _temporaryFolder = string.Empty;

        public string TemporaryFolder {
            get => _temporaryFolder;
            set => SetProperty(ref _temporaryFolder, value);
        }

        private string _modelFormat;

        public string ModelFormat {
            get => _modelFormat;
            set => SetProperty(ref _modelFormat, value);
        }

        private int _maxBatchCount = 100;

        public int MaxBatchCount {
            get => _maxBatchCount;
            set => SetProperty(ref _maxBatchCount, value);
        }


        public ICommand OpenCommand { get; private set; }
        public ICommand OpenTemporaryCommand { get; private set; }
        public ICommand ClearTemporaryCommand { get; private set; }

        private async void TapOpen()
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            App.ViewModel.InitializePicker(picker);
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            foreach (var item in await folder.GetFilesAsync())
            {
                if (item.Name.StartsWith("ffmpeg"))
                {
                    FFmpegBin = folder.Path;
                    return;
                }
            }
        }

        private async void TapOpenTemporary()
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            App.ViewModel.InitializePicker(picker);
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            TemporaryFolder = folder.Path;
        }
        private async void TapClearTemporary()
        {
            ITemporaryStorage storage = string.IsNullOrWhiteSpace(TemporaryFolder) ? new TemporaryStorage() : new Shared.IO.TemporaryStorage(TemporaryFolder);
            await storage.ClearAsync();
            App.ViewModel.Setting.Delete(key => key.StartsWith('_'));
            App.ViewModel.Success("清除缓存完成");
        }

        public async Task SaveAsync()
        {
            _setting.Set(SettingNames.FFmpegPath, FFmpegBin);
            _setting.Set(SettingNames.TemporaryPath, TemporaryFolder);
            _setting.Set(SettingNames.ModelFormat, ModelFormat);
            _setting.Set(SettingNames.MaxBatchCount, MaxBatchCount);
            await _setting.SaveAsync();
        }
    }
}
