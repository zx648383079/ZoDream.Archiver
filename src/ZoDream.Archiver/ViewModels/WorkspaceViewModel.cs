using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using ZoDream.Archiver.Dialogs;
using ZoDream.Shared;
using ZoDream.Shared.Compression;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class WorkspaceViewModel : BindableBase
    {
        public WorkspaceViewModel()
        {
            AddCommand = new RelayCommand(TapAdd);
            DeleteCommand = new RelayCommand(TapDelete);
            ViewCommand = new RelayCommand(TapView);
            InfoCommand = new RelayCommand(TapInfo);
            FindCommand = new RelayCommand(TapFind);
            SaveCommand = new RelayCommand(TapSaveAs);
            DragCommand = new RelayCommand(TapDrag);
            StopFilterCommand = new RelayCommand(TapStopFilter);
        }

        private IArchiveOptions? _options;
        private IStorageFile _storageFile;
        private IArchiveScheme? _scheme;


        private IList<IReadOnlyEntry> _entryItems = [];

        private ObservableCollection<IReadOnlyEntry> _filteredItems = [];

        public ObservableCollection<IReadOnlyEntry> FilteredItems {
            get => _filteredItems;
            set => Set(ref _filteredItems, value);
        }

        private IReadOnlyEntry? _selectedItem;

        public IReadOnlyEntry? SelectedItem {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }

        private bool _isFiltered;

        public bool IsFiltered {
            get => _isFiltered;
            set => Set(ref _isFiltered, value);
        }


        public bool IsEncrypted => _entryItems.Where(i => i.IsEncrypted).Any();

        public ICommand AddCommand { get; private set; }
        public ICommand ViewCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand FindCommand { get; private set; }
        public ICommand StopFilterCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DragCommand { get; private set; }



        private async void TapSaveAs(object? _)
        {
            if (!_entryItems.Any())
            {
                return;
            }
            var app = App.ViewModel;
            var picker = new ExtractDialog();
            var model = picker.ViewModel;
            if (_options is not null)
            {
                model.Password = _options.Password ?? string.Empty;
                model.DictFileName = _options.Dictionary ?? string.Empty;
            }
            model.IsEncrypted = IsEncrypted ? Visibility.Visible : Visibility.Collapsed;
            model.IsSelected = SelectedItem is not null ? Visibility.Visible : Visibility.Collapsed;
            var res = await app.OpenDialogAsync(picker);
            if (res != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary
                || !model.Verify())
            {
                return;
            }
            if (IsEncrypted)
            {
                _options = new ArchiveOptions(model.Password, model.DictFileName);
            }
            var token = app.ShowProgress("解压中...");
            await Task.Factory.StartNew(async () => {
                using var fs = (await _storageFile.OpenReadAsync()).AsStreamForRead();
                using var reader = _scheme?.Open(fs, _storageFile.Path, _storageFile.Name, 
                    _options);
                try
                {
                    if (SelectedItem is not null && model.OnlySelected)
                    {
                        reader?.ExtractToDirectory(SelectedItem, model.FileName, app.UpdateProgress, token);
                    }
                    else
                    {
                        reader?.ExtractToDirectory(model.FileName, model.ExtractMode, app.UpdateProgress, token);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                app.CloseProgress();
            }, token);
        }

        private void TapDelete(object? _)
        {

        }
        private void TapDrag(object? _)
        {

        }
        private void TapAdd(object? _)
        {

        }
        private void TapView(object? _)
        {

        }
        private void TapInfo(object? _)
        {

        }
        private async void TapFind(object? _)
        {
            var picker = new SearchDialog();
            var res = await App.ViewModel.OpenDialogAsync(picker);
            if (res != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return;
            }
            var filter = picker.ViewModel;
            var items = _entryItems.Where(i => filter.IsMatch(i.Name));
            if (items.Count() < _entryItems.Count)
            {
                for (var i = FilteredItems.Count - 1; i >= 0; i--)
                {
                    if (!items.Contains(FilteredItems[i]))
                    {
                        FilteredItems.RemoveAt(i);
                    }
                }
            }
            foreach (var item in items)
            {
                if (!FilteredItems.Contains(item))
                {
                    FilteredItems.Add(item);
                }
            }
            IsFiltered = FilteredItems.Count != _entryItems.Count;
            if (SelectedItem is not null && !FilteredItems.Contains(SelectedItem))
            {
                SelectedItem = null;
            }
        }


        private void TapStopFilter(object? _)
        {
            IsFiltered = false;
            foreach (var item in _entryItems)
            {
                if (!FilteredItems.Contains(item))
                {
                    FilteredItems.Add(item);
                }
            }
        }

        public void LoadAsync(object arg)
        {
            if (arg is IStorageFile file)
            {
                _storageFile = file;
                RefreshEntry();
                return;
            }
            if (arg is IReadOnlyList<IStorageItem> items)
            {
                foreach (var item in items)
                {
                    if (item is IStorageFile f)
                    {
                        _storageFile = f;
                        RefreshEntry();
                        return;
                    }
                }
            }
        }

        private async void RefreshEntry()
        {
            if (_storageFile is null)
            {
                return;
            }
            var app = App.ViewModel;
            using var fs = (await _storageFile.OpenReadAsync()).AsStreamForRead();
            ReadBegin:
            try
            {
                if (!TryReadEntry(fs)) 
                {
                    await app.ConfirmAsync("不支持文件");
                    app.NavigateBack();
                }
                return;
            }
            catch (Exception ex)
            {
                if (!CompressScheme.IsCryptographicException(ex))
                {
                    await app.ConfirmAsync("文件解析失败");
                    app.NavigateBack();
                    return;
                }
            }
            if (!await OpenPasswordAsync())
            {
                app.NavigateBack();
                return;
            }
            goto ReadBegin;
        }

        private bool TryReadEntry(Stream fs)
        {
            IArchiveReader? reader;
            if (_scheme is null)
            {
                reader = App.ViewModel.Plugin.TryGetReader(fs, _storageFile.Path, _options, out _scheme);
            } else
            {
                reader = _scheme.Open(fs, _storageFile.Path, _storageFile.Name, _options);
            }
            if (reader is null)
            {
                return false;
            }
            LoadEntry(reader!);
            reader?.Dispose();
            return true;
        }

        private async Task<bool> OpenPasswordAsync()
        {
            var picker = new PasswordDialog();
            var res = await App.ViewModel.OpenDialogAsync(picker);
            if (res != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return false;
            }
            if ((_entryItems.Count > 0 && string.IsNullOrWhiteSpace(picker.ViewModel.Password)) 
                || (_entryItems.Count == 0 && string.IsNullOrWhiteSpace(picker.ViewModel.DictFileName)))
            {
                return false;
            }
            _options = new ArchiveOptions(picker.ViewModel.Password, picker.ViewModel.DictFileName);
            return true;
        }

        private void LoadEntry(IArchiveReader reader)
        {
            _entryItems.Clear();
            foreach (var item in reader.ReadEntry())
            {
                if (item is null)
                {
                    continue;
                }
                _entryItems.Add(item);
            }
            TapStopFilter(null);
        }
    }
}
