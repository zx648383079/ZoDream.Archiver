using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class PasswordDialogViewModel: ObservableObject, IFormValidator, IEntryConfiguration
    {
        public PasswordDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
        }

        private string _password = string.Empty;

        public string Password {
            get => _password;
            set {
                SetProperty(ref _password, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private string _dictFileName = string.Empty;

        public string DictFileName {
            get => _dictFileName;
            set {
                SetProperty(ref _dictFileName, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(DictFileName);

        public ICommand OpenCommand { get; private set; }

        private async void TapOpen()
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

        public void Load(IEntryService service, object options)
        {
            if (options is IArchiveOptions o)
            {
                Password = o.Password ?? string.Empty;
                DictFileName = o.Dictionary ?? string.Empty;
            }
        }

        public void Unload(IEntryService service, object options)
        {
            var type = options.GetType();
            var property = type.GetProperty(nameof(Password));
            property?.SetValue(options, Password);
            property = type.GetProperty("Dictionary");
            property?.SetValue(options, DictFileName);
        }
    }
}
