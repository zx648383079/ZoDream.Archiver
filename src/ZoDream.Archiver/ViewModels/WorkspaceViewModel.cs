using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using ZoDream.Archiver.Dialogs;
using ZoDream.Shared.Compression;
using ZoDream.Shared.Interfaces;
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
        }

        private IArchiveOptions? _options;
        private IStorageFile _storageFile;
        private IArchiveScheme? _scheme;

        private ObservableCollection<IReadOnlyEntry> _entryItems = [];

        public ObservableCollection<IReadOnlyEntry> EntryItems {
            get => _entryItems;
            set => Set(ref _entryItems, value);
        }

        public bool IsEncrypted => EntryItems.Where(i => i.IsEncrypted).Any();

        public ICommand AddCommand { get; private set; }
        public ICommand ViewCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand FindCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DragCommand { get; private set; }

        private async void TapSaveAs(object? _)
        {
            if (!EntryItems.Any())
            {
                return;
            }
            if (IsEncrypted && _options is null)
            {
                if (!await OpenPasswordAsync())
                {
                    return;
                }
            }
            var app = App.ViewModel;
            var picker = new FolderPicker();
            app.InitializePicker(picker);
            // picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var folder = await picker.PickSingleFolderAsync();
            if (folder is null)
            {
                return;
            }
            var token = app.ShowProgress("解压中...");
            await Task.Factory.StartNew(async () => {
                using var fs = (await _storageFile.OpenReadAsync()).AsStreamForRead();
                using var reader = _scheme?.Open(fs, _storageFile.Path, _storageFile.Name, 
                    _options);
                try
                {
                    reader?.ExtractToDirectory(folder.Path, app.UpdateProgress, token);
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
        private void TapFind(object? _)
        {

        }

        public void LoadAsync(object arg)
        {
            if (arg is IStorageFile file)
            {
                _storageFile = file;
                RefreshEntry();
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
            catch (IndexOutOfRangeException)
            {
                await app.ConfirmAsync("文件解析失败");
                app.NavigateBack();
                return;
            }
            catch (Exception)
            {
                
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
            if ((EntryItems.Count > 0 && string.IsNullOrWhiteSpace(picker.ViewModel.Password)) 
                || (EntryItems.Count == 0 && string.IsNullOrWhiteSpace(picker.ViewModel.DictFileName)))
            {
                return false;
            }
            _options = new ArchiveOptions(picker.ViewModel.Password, picker.ViewModel.DictFileName);
            return true;
        }

        private void LoadEntry(IArchiveReader reader)
        {
            EntryItems.Clear();
            foreach (var item in reader.ReadEntry())
            {
                if (item is null)
                {
                    continue;
                }
                EntryItems.Add(item);
            }
        }
    }
}
