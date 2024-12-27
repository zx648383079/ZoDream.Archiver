using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class ExplorerViewModel: BindableBase
    {
        public ExplorerViewModel()
        {
            AddCommand = new RelayCommand(TapAdd);
            AddFolderCommand = new RelayCommand(TapAddFolder);
            DeleteCommand = new StandardUICommand(StandardUICommandKind.Delete)
            {
                Command = new RelayCommand(TapDelete)
            };
            SaveCommand = new StandardUICommand(StandardUICommandKind.Save) 
            {
                Command = new RelayCommand(TapSaveAs)
            };
            ViewCommand = new StandardUICommand(StandardUICommandKind.Open)
            {
                Command = new RelayCommand(TapView)
            };
            BackCommand = new StandardUICommand(StandardUICommandKind.Backward)
            {
                Command = new RelayCommand(TapBack)
            };
            DragCommand = new RelayCommand<IEnumerable<IStorageItem>>(TapDrag);
            _storage = new StorageExplorer(_service);
        }

        private readonly AppViewModel _app = App.ViewModel;
        private readonly List<ISourceEntry> _routeItems = [];
        private readonly IEntryService _service = new EntryService(App.ViewModel.Logger);
        private readonly StorageExplorer _storage;
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

        public bool CanGoBack => _routeItems.Count > 0;


        public ICommand AddCommand { get; private set; }
        public ICommand AddFolderCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DragCommand { get; private set; }
        public ICommand ViewCommand { get; private set; }
        public ICommand BackCommand { get; private set; }

        private void TapBack(object? _)
        {

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
            _storage.Add(items.Select(i => i.Path));
            if (CanGoBack)
            {
                return;
            }
            Open(new DirectoryEntry(string.Empty));
        }

        private void TapDrag(IEnumerable<IStorageItem>? items)
        {
            if (items is null)
            {
                return;
            }
            _storage.Add(items.Select(i => i.Path));
            if (CanGoBack)
            {
                return;
            }
            Open(new DirectoryEntry(string.Empty));
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
            _storage.Add(folder.Path);
            if (CanGoBack)
            {
                return;
            }
            Open(new DirectoryEntry(string.Empty));
        }

        private void Open(ISourceEntry entry)
        {
            var e = _storage.Open(entry);
            if (e is DirectoryEntryStream items)
            {
                FileItems.Clear();
                foreach (var item in items.Items)
                {
                    FileItems.Add(new EntryViewModel(item));
                }
                return;
            }
        }

        private async void TapView(object? _)
        {

        }

        private async void TapSaveAs(object? _)
        {

        }

        private void TapDelete(object? _)
        {
            if (SelectedItem is null || CanGoBack)
            {
                return;
            }
            _storage.Remove(SelectedItem.FullPath);
            FileItems.Remove(SelectedItem);
            SelectedItem = null;
        }
    }
}
