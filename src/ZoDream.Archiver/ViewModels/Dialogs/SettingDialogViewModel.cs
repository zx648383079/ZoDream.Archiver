using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class SettingDialogViewModel : BindableBase
    {

        public SettingDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
            OpenTemporaryCommand = new RelayCommand(TapOpenTemporary);
            ClearTemporaryCommand = new RelayCommand(TapClearTemporary);
            _setting = App.ViewModel.Setting;
            FFmpegBin = _setting.Get<string>(SettingNames.FFmpegPath);
            TemporaryFolder = _setting.Get<string>(SettingNames.TemporaryPath);
        }

        private readonly ISettingContainer _setting;


        private string _ffmpegBin = string.Empty;

        public string FFmpegBin {
            get => _ffmpegBin;
            set => Set(ref _ffmpegBin, value);
        }

        private string _temporaryFolder = string.Empty;

        public string TemporaryFolder {
            get => _temporaryFolder;
            set => Set(ref _temporaryFolder, value);
        }


        public ICommand OpenCommand { get; private set; }
        public ICommand OpenTemporaryCommand { get; private set; }
        public ICommand ClearTemporaryCommand { get; private set; }

        private async void TapOpen(object? _)
        {
            var picker = new FolderPicker();
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

        private async void TapOpenTemporary(object? _)
        {
            var picker = new FolderPicker();
            App.ViewModel.InitializePicker(picker);
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            TemporaryFolder = folder.Path;
        }
        private void TapClearTemporary(object? _)
        {
            if (string.IsNullOrWhiteSpace(TemporaryFolder))
            {
                return;
            }
            var storage = new TemporaryStorage(TemporaryFolder);
            storage.Clear();
        }

        public async Task SaveAsync()
        {
            _setting.Set(SettingNames.FFmpegPath, FFmpegBin);
            _setting.Set(SettingNames.TemporaryPath, TemporaryFolder);
            await _setting.SaveAsync();
        }
    }
}
