using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using ZoDream.Archiver.Dialogs;
using ZoDream.BundleExtractor;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class BundleViewModel : BindableBase
    {
        public BundleViewModel()
        {
            AddCommand = new RelayCommand(TapAdd);
            AddFolderCommand = new RelayCommand(TapAddFolder);
            DeleteCommand = new RelayCommand(TapDelete);
            SaveCommand = new RelayCommand(TapSaveAs);
            ViewCommand = new RelayCommand(TapView);
            SettingCommand = new RelayCommand(TapSetting);
            DragCommand = new RelayCommand<IEnumerable<IStorageItem>>(TapDrag);
            _scheme = new(_app.Logger);
        }

        private readonly AppViewModel _app = App.ViewModel;
        private readonly BundleScheme _scheme;
        private IBundleOptions? _options;

        private ObservableCollection<EntryViewModel> _fileItems = [];

        public ObservableCollection<EntryViewModel> FileItems {
            get => _fileItems;
            set => Set(ref _fileItems, value);
        }

        private EntryViewModel? _selectedItem;

        public EntryViewModel? SelectedItem {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }

        public ICommand AddCommand { get; private set; }
        public ICommand AddFolderCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DragCommand { get; private set; }
        public ICommand ViewCommand { get; private set; }

        public ICommand SettingCommand { get; private set; }

        private async void TapSetting(object? _)
        {
            var picker = new SettingDialog();
            var res = await _app.OpenDialogAsync(picker);
            if (res != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return;
            }
            await picker.ViewModel.SaveAsync();
        }
        private async void TapView(object? _)
        {
            if (FileItems.Count == 0)
            {
                await _app.ConfirmAsync("请选择文件");
                return;
            }
            _options ??= _scheme.TryLoad(FileItems.Select(i => i.FullPath));
            var dialog = new PropertyDialog();
            dialog.ViewModel.Load(_options);
            await _app.OpenDialogAsync(dialog);
        }

        private async void TapAdd(object? _)
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

        private async void TapAddFolder(object? _)
        {
            var picker = new FolderPicker();
            _app.InitializePicker(picker);
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            AddFile([folder.Path]);
        }

        private async void TapSaveAs(object? _)
        {
            var fileItems = FileItems.Select(i => i.FullPath).ToArray();
            if (!fileItems.Any())
            {
                await _app.ConfirmAsync("请选择文件");
                return;
            }
            var picker = new BundleDialog();
            var model = picker.ViewModel;
            model.Load(_scheme, _options);
            var res = await _app.OpenFormAsync(picker);
            if (!res)
            {
                _app.Warning("已取消操作！");
                return;
            }
            _options ??= new BundleOptions();
            model.Unload(_options);
            var token = _app.ShowProgress("解压中...");
            _ = Task.Factory.StartNew(() => {
                var progress = 0D;
                while (!token.IsCancellationRequested)
                {
                    Thread.Sleep(500);
                    if (progress < .95)
                    {
                        progress += .001;
                    }
                    _app.UpdateProgress(progress);
                }
            }, token);
            await Task.Factory.StartNew(() => {
                var watch = new Stopwatch();
                watch.Start();
                IBundleReader? reader;
                reader = _scheme.Load(fileItems, _options);
                try
                {
                    reader?.ExtractTo(model.FileName, model.ExtractMode, token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                reader?.Dispose();
                watch.Stop();
                Debug.WriteLine($"Use Time: {watch.Elapsed.TotalSeconds}");
                _app.CloseProgress();
                _app.Success("已完成操作！");
            }, token);
            
        }

        private void TapDelete(object? _)
        {
            if (SelectedItem is null)
            {
                return;
            }
            FileItems.Remove(SelectedItem);
            SelectedItem = null;
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
            _scheme?.Dispose();
        }
    }
}
