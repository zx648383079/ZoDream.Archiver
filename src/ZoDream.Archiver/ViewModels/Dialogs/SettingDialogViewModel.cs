using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            _setting = App.ViewModel.Setting;
            FFmpegBin = _setting.Get<string>(SettingNames.FFmpegPath);
        }

        private readonly ISettingContainer _setting;


        private string _ffmpegBin = string.Empty;

        public string FFmpegBin {
            get => _ffmpegBin;
            set => Set(ref _ffmpegBin, value);
        }

        public ICommand OpenCommand { get; private set; }

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


        public async Task SaveAsync()
        {
            _setting.Set(SettingNames.FFmpegPath, FFmpegBin);
            await _setting.SaveAsync();
        }
    }
}
