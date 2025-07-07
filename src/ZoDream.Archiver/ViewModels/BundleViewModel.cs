using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using ZoDream.Archiver.Controls;
using ZoDream.Archiver.Dialogs;
using ZoDream.BundleExtractor;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Media;

namespace ZoDream.Archiver.ViewModels
{
    public class BundleViewModel : ObservableObject
    {
        public BundleViewModel()
        {
            AddCommand = UICommand.Add(TapAdd);
            AddFolderCommand = UICommand.AddFolder(TapAddFolder);
            DeleteCommand = UICommand.Delete(TapDelete);
            
            SaveCommand = UICommand.Save(TapSaveAs);
            ViewCommand = UICommand.View(TapView);
            SettingCommand = UICommand.Setting(TapSetting);
            DragCommand = new RelayCommand<IEnumerable<IStorageItem>>(TapDrag);
            ExplorerCommand = UICommand.Index(TapExplorer);
            CodeCommand = UICommand.Code(TapCode);
            LogCommand = UICommand.Log(TapLog);
            DumpCommand = UICommand.Dump(TapDump);
            _service = _app.Service;
            _scheme = new(_service);
            LoadSetting();
        }

        private readonly AppViewModel _app = App.ViewModel;
        private readonly IEntryService _service;
        private readonly BundleScheme _scheme;
        private readonly BundleOptions _options = new();

        private ObservableCollection<EntryViewModel> _fileItems = [];

        public ObservableCollection<EntryViewModel> FileItems {
            get => _fileItems;
            set => SetProperty(ref _fileItems, value);
        }

        private IEntryItem[]? _selectedItems;

        public IEntryItem[]? SelectedItems {
            get => _selectedItems;
            set => SetProperty(ref _selectedItems, value);
        }

        private bool _isMultipleSelect;

        public bool IsMultipleSelect {
            get => _isMultipleSelect;
            set => SetProperty(ref _isMultipleSelect, value);
        }


        public ICommand AddCommand { get; private set; }
        public ICommand AddFolderCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DragCommand { get; private set; }
        public ICommand ViewCommand { get; private set; }

        public ICommand SettingCommand { get; private set; }
        public ICommand DumpCommand { get; private set; }

        public ICommand ExplorerCommand {  get; private set; }
        public ICommand CodeCommand {  get; private set; }
        public ICommand LogCommand {  get; private set; }

        private void TapLog()
        {
            Process.Start("explorer", $"/select,{AppViewModel.LogFileName}");
        }

        private async void TapCode()
        {
            var picker = new CodeDialog();
            var res = await _app.OpenDialogAsync(picker);
            if (res != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return;
            }
            if (picker.ViewModel.SaveAs())
            {
                _app.Success("已完成代码生成！");
            }

        }

        private async void TapDump()
        {
            var picker = new DumpDialog();
            var model = picker.ViewModel;
            model.Load(_options);
            if (!await _app.OpenFormAsync(picker))
            {
                return;
            }
            await model.SaveAsAsync();
        }

        private async void TapExplorer()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".bin");
            picker.FileTypeFilter.Add(".json");
            _app.InitializePicker(picker);
            var res = await picker.PickSingleFileAsync();
            if (res is null)
            {
                return;
            }
            var dialog = new EntryDialog();
            dialog.ViewModel.Load(res.Path);
            await _app.OpenDialogAsync(dialog);
        }

        private async void TapSetting()
        {
            var picker = new SettingDialog();
            var res = await _app.OpenDialogAsync(picker);
            if (res != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return;
            }
            await picker.ViewModel.SaveAsync();
            LoadSetting();
        }

        private void LoadSetting()
        {
            var temporary = _app.Setting.Get<string>(SettingNames.TemporaryPath);
            if (!string.IsNullOrWhiteSpace(temporary))
            {
                _service.Add<ITemporaryStorage>(new Shared.IO.TemporaryStorage(temporary));
            }
            FFmpegBinariesHelper.RegisterFFmpegBinaries(
                _app.Setting.Get<string>(SettingNames.FFmpegPath));
            _options.ModelFormat = _app.Setting.Get<string>(SettingNames.ModelFormat);
            _options.MaxBatchCount = _app.Setting.Get<int>(SettingNames.MaxBatchCount);
            if (_options.MaxBatchCount < 1)
            {
                _options.MaxBatchCount = 100;
            }
            var history = _app.Setting.Get<string>(SettingNames.BundleHistory);
            if (!string.IsNullOrWhiteSpace(history))
            {
                var args = history.Split(';');
                _options.Engine = args[0];
                _options.Platform = args[1];
            }
        }

        private async void TapView()
        {
            if (FileItems.Count == 0)
            {
                await _app.ConfirmAsync("请选择文件");
                return;
            }
            _options.Load(_scheme.TryLoad(FileItems.Select(i => i.FullPath)));
            var dialog = new BundlePropertyDialog();
            dialog.ViewModel.Load(_options);
            await _app.OpenDialogAsync(dialog);
        }

        private async void TapAdd()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");
            _app.InitializePicker(picker);
            var items = await picker.PickMultipleFilesAsync();
            if (items is null)
            {
                return;
            }
            AddFile(items.Select(x => x.Path));
        }

        private void AddFile(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                AddFile(item);
            }
        }

        private bool HasFile(string fileName)
        {
            return FileItems.Where(i => i.FullPath == fileName).Any();
        }

        private void AddFile(string fileName)
        {
            if (HasFile(fileName))
            {
                return;
            }
            FileItems.Add(new EntryViewModel(fileName));
        }

        private void AddFile(string name, string fileName)
        {
            if (HasFile(fileName))
            {
                return;
            }
            FileItems.Add(new EntryViewModel(name, fileName));
        }

        private void TapDrag(IEnumerable<IStorageItem>? items)
        {
            if (items is null)
            {
                return;
            }
            AddFile(items.Select(x => x.Path));
        }

        private async void TapAddFolder()
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            _app.InitializePicker(picker);
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            AddFile([folder.Path]);
        }

        private async void TapSaveAs()
        {
            var fileItems = FileItems.Select(i => i.FullPath).ToArray();
            if (fileItems.Length == 0)
            {
                await _app.ConfirmAsync("请选择文件");
                return;
            }
            // TODO 判断是否存在历史记录，询问是否继续
            var picker = new BundleDialog();
            var model = picker.ViewModel;
            model.Load(_scheme, _options);
            var res = await _app.OpenFormAsync(picker);
            if (!res)
            {
                // _app.Warning("已取消操作！");
                return;
            }
            model.Unload(_options);
            _app.Setting.Set(SettingNames.BundleHistory, $"{_options.Engine};{_options.Platform}");
            var source = new BundleSource(fileItems);
            if (!_options.OnlyDependencyTask && _service.CheckPoint(source.GetHashCode())
                && !await _app.ConfirmAsync("是否继续上次任务？"))
            {
                _service.SavePoint(source.GetHashCode(), 0);
            }
            var token = _app.OpenProgress("解压中...");
            await Task.Factory.StartNew(() => {
                IBundleReader? reader;
                reader = _scheme.Load(source, _options);
                try
                {
                    reader?.ExtractTo(model.FileName, model.ExtractMode, token);
                }
                catch (Exception ex)
                {
                    _app.Logger.Log(ex);
                }
                reader?.Dispose();
                _app.CloseProgress();
                _app.Success("已完成操作！");
                _app.Logger.Flush();
            }, token);
        }

        private void TapDelete()
        {
            if (SelectedItems is null)
            {
                return;
            }
            foreach (var item in SelectedItems)
            {
                FileItems.Remove(item as EntryViewModel);
            }
            SelectedItems = null;
        }


        public void LoadAsync(object arg)
        {
            if (arg is IStorageItem file)
            {
                TapDrag([file]);
                return;
            }
            if (arg is IReadOnlyList<IStorageItem> items)
            {
                TapDrag(items);
            }
        }

        public void UnloadAsync()
        {
            _service.Dispose();
            _scheme?.Dispose();
        }
    }
}
