using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.Interfaces;
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
            Format3D = _setting.Get<bool>(SettingNames.Format3D);
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

        private bool _3DFormat;

        public bool Format3D {
            get => _3DFormat;
            set => Set(ref _3DFormat, value);
        }


        public ICommand OpenCommand { get; private set; }
        public ICommand OpenTemporaryCommand { get; private set; }
        public ICommand ClearTemporaryCommand { get; private set; }

        private async void TapOpen(object? _)
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

        private async void TapOpenTemporary(object? _)
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
        private async void TapClearTemporary(object? _)
        {
            ITemporaryStorage storage = string.IsNullOrWhiteSpace(TemporaryFolder) ? new TemporaryStorage() : new Shared.IO.TemporaryStorage(TemporaryFolder);
            await storage.ClearAsync();
            App.ViewModel.Success("清除缓存完成");
        }

        public async Task SaveAsync()
        {
            _setting.Set(SettingNames.FFmpegPath, FFmpegBin);
            _setting.Set(SettingNames.TemporaryPath, TemporaryFolder);
            _setting.Set(SettingNames.Format3D, Format3D);
            await _setting.SaveAsync();
        }
    }
}
