using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class PasswordDialogViewModel: BindableBase, IFormValidator
    {
        public PasswordDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
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

        public bool IsValid => !string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(DictFileName);

        public ICommand OpenCommand { get; private set; }

        private async void TapOpen(object? _)
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
