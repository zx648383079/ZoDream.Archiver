using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Il2CppDumper;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class DumpDialogViewModel : ObservableObject, IDumpOptions
    {
        public DumpDialogViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
            MetadataCommand = new RelayCommand(TapMetadata);
            Il2cppCommand = new RelayCommand(TapIl2cpp);
        }

        private readonly AppViewModel _app = App.ViewModel;

        private bool _dumpMethod = true;
        private bool _dumpField = true;
        private bool _dumpProperty = false;
        private bool _dumpAttribute = false;
        private bool _dumpFieldOffset = true;
        private bool _dumpMethodOffset = true;
        private bool _dumpTypeDefIndex = true;
        private bool _generateDummyDll = true;
        private bool _generateStruct = true;
        private bool _dummyDllAddToken = true;
        private bool _requireAnyKey = true;
        private bool _forceIl2CppVersion = false;
        private double _forceVersion = 24.3;
        private bool _forceDump = false;
        private bool _noRedirectedPointer = false;

        public bool DumpMethod { 
            get => _dumpMethod; 
            set => SetProperty(ref _dumpMethod, value); 
        }
        public bool DumpField { 
            get => _dumpField; 
            set => SetProperty(ref _dumpField, value); 
        }
        public bool DumpProperty { get => _dumpProperty; set => SetProperty(ref _dumpProperty, value); }
        public bool DumpAttribute { get => _dumpAttribute; set => SetProperty(ref _dumpAttribute, value); }
        public bool DumpFieldOffset { get => _dumpFieldOffset; set => SetProperty(ref _dumpFieldOffset, value); }
        public bool DumpMethodOffset { get => _dumpMethodOffset; set => SetProperty(ref _dumpMethodOffset, value); }
        public bool DumpTypeDefIndex { get => _dumpTypeDefIndex; set => SetProperty(ref _dumpTypeDefIndex, value); }
        public bool GenerateDummyDll { get => _generateDummyDll; set => SetProperty(ref _generateDummyDll, value); }
        public bool GenerateStruct { get => _generateStruct; set => SetProperty(ref _generateStruct, value); }
        public bool DummyDllAddToken { get => _dummyDllAddToken; set => SetProperty(ref _dummyDllAddToken, value); }
        public bool RequireAnyKey { get => _requireAnyKey; set => SetProperty(ref _requireAnyKey, value); }
        public bool ForceIl2CppVersion { get => _forceIl2CppVersion; set => SetProperty(ref _forceIl2CppVersion, value); }
        public double ForceVersion { get => _forceVersion; set => SetProperty(ref _forceVersion, value); }
        public bool ForceDump { get => _forceDump; set => SetProperty(ref _forceDump, value); }
        public bool NoRedirectedPointer { get => _noRedirectedPointer; set => SetProperty(ref _noRedirectedPointer, value); }

        private string _metadatPath = string.Empty;

        public string MetadataPath {
            get => _metadatPath;
            set {
                SetProperty(ref _metadatPath, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private string _il2cppPath = string.Empty;

        public string Il2cppPath {
            get => _il2cppPath;
            set {
                SetProperty(ref _il2cppPath, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private string _dumpAddress = string.Empty;

        public string DumpAddress {
            get => _dumpAddress;
            set => SetProperty(ref _dumpAddress, value);
        }

        private string _outputFolder = string.Empty;

        public string OutputFolder {
            get => _outputFolder;
            set {
                SetProperty(ref _outputFolder, value);
                OnPropertyChanged(nameof(IsValid));
            }
        }


        public bool IsValid => !string.IsNullOrWhiteSpace(MetadataPath) 
            && !string.IsNullOrWhiteSpace(Il2cppPath)
            && !string.IsNullOrWhiteSpace(OutputFolder);

        public ICommand OpenCommand { get; private set; }
        public ICommand MetadataCommand { get; private set; }
        public ICommand Il2cppCommand { get; private set; }

        private async void TapOpen()
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            _app.InitializePicker(picker);
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            OutputFolder = folder.Path;
        }
        private async void TapMetadata()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".dat");
            _app.InitializePicker(picker);
            var res = await picker.PickSingleFileAsync();
            if (res is null)
            {
                return;
            }
            MetadataPath = res.Path;
        }
        private async void TapIl2cpp()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".so");
            picker.FileTypeFilter.Add(".dll");
            _app.InitializePicker(picker);
            var res = await picker.PickSingleFileAsync();
            if (res is null)
            {
                return;
            }
            Il2cppPath = res.Path;
        }

        public void Load(IArchiveExtractOptions options)
        {
            OutputFolder = options.OutputFolder;
            if (options is IBundleOptions b && !string.IsNullOrWhiteSpace(b.Entrance))
            {
                MetadataPath = Directory.GetFiles(b.Entrance, "global-metadata.dat", SearchOption.AllDirectories).FirstOrDefault() ?? string.Empty;
                Il2cppPath = Directory.GetFiles(b.Entrance, "*il2cpp.*", SearchOption.AllDirectories).FirstOrDefault() ?? string.Empty;
            }
        }

        public async Task<bool> SaveAsAsync()
        {
            if (!IsValid)
            {
                return false;
            }
            var token = _app.OpenProgress("解压中...");
            if (!ulong.TryParse(DumpAddress, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var dumpAddress))
            {
                dumpAddress = 0;
            }
            if (dumpAddress > 0)
            {
                ForceDump = true;
            }
            await Task.Factory.StartNew(() => {
                try
                {
                    using var dumper = new ConvertDumper(_app.Logger, this);
                    dumper.Initialize(Il2cppPath, MetadataPath, dumpAddress);
                    dumper.SaveAs(OutputFolder, Shared.Models.ArchiveExtractMode.Overwrite);
                }
                catch (Exception ex)
                {
                    _app.Logger.Log(ex);
                }
                _app.CloseProgress();
                _app.Success("已完成操作！");
                _app.Logger.Flush();
            }, token);
            
            return true;
        }
    }
}
