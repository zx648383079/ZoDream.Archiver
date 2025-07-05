using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.Converters;
using ZoDream.SourceGenerator;

namespace ZoDream.Archiver.ViewModels
{
    public class CodeDialogViewModel : ObservableObject, IFormValidator
    {
        public CodeDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
            OpenOutputCommand = new RelayCommand(TapOpenOutput);
        }

        private string[] _languageItems = ["c#", "imhex"];

        public string[] LanguageItems {
            get => _languageItems;
            set => SetProperty(ref _languageItems, value);
        }


        private string _fileName = string.Empty;

        public string FileName {
            get => _fileName;
            set {
                SetProperty(ref _fileName, value);
                OnPropertyChanged(nameof(IsValid));
                if (!value.EndsWith(".json"))
                {
                    LanguageIndex = 0;
                }
            }
        }

        private int _languageIndex;

        public int LanguageIndex {
            get => _languageIndex;
            set => SetProperty(ref _languageIndex, value);
        }

        private string _packageName = string.Empty;

        public string PackageName {
            get => _packageName;
            set => SetProperty(ref _packageName, value);
        }


        private string _version = string.Empty;

        public string Version {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        private string _outputFileName = string.Empty;

        public string OutputFileName {
            get => _outputFileName;
            set {
                SetProperty(ref _outputFileName, value);
                OnPropertyChanged(nameof(IsValid));
                LanguageIndex = value.EndsWith(".cs") ? 0 : 1;
            }
        }


        public bool IsValid => !string.IsNullOrWhiteSpace(FileName) && !string.IsNullOrWhiteSpace(OutputFileName);

        public ICommand OpenCommand { get; private set; }
        public ICommand OpenOutputCommand { get; private set; }

        private async void TapOpen()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".json");
            picker.FileTypeFilter.Add(".hexpat");
            picker.FileTypeFilter.Add(".pat");
            App.ViewModel.InitializePicker(picker);
            var res = await picker.PickSingleFileAsync();
            if (res is null)
            {
                return;
            }
            FileName = res.Path;
        }

        private async void TapOpenOutput()
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("C#", [".cs"]);
            picker.FileTypeChoices.Add("ImHex", [".hexpat", ".pat"]);
            picker.SuggestedFileName = StringConverter.Studly(Path.GetFileNameWithoutExtension(FileName)) + ".g";
            App.ViewModel.InitializePicker(picker);
            var res = await picker.PickSaveFileAsync();
            if (res is null)
            {
                return;
            }
            OutputFileName = res.Path;
        }

        internal bool SaveAs()
        {
            if (!IsValid)
            {
                return false;
            }
            using var fs = File.OpenRead(FileName);
            using var fr = File.Create(OutputFileName);
            switch(Path.GetExtension(FileName).ToLower())
            {
                case ".json":
                    var data = new TypeNodeReader(fs).Read();
                    if (LanguageIndex < 1)
                    {
                        new TypeSourceWriter(data).Write(fr);
                    } else
                    {
                        new PatternLanguageWriter(data).Write(fr);
                    }
                    break;
                case ".hexpat" or ".pat":
                    var lexer = new PatternLanguageLexer(new StreamReader(fs));
                    new SourceWriter(lexer).Write(fr);
                    break;
            }
            return true;
        }
    }
}
