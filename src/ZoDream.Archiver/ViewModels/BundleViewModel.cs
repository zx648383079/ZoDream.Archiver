using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using ZoDream.Archiver.Dialogs;
using ZoDream.BundleExtractor;
using ZoDream.Shared.Models;
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
            DragCommand = new RelayCommand<IEnumerable<IStorageItem>>(TapDrag);
        }

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
        private void TapView(object? _)
        {

        }
        private async void TapAdd(object? _)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");
            App.ViewModel.InitializePicker(picker);
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
            App.ViewModel.InitializePicker(picker);
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
                return;
            }
            var app = App.ViewModel;
            var picker = new ExtractDialog();
            var model = picker.ViewModel;
            var res = await app.OpenDialogAsync(picker);
            if (res != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary
                || !model.Verify())
            {
                return;
            }

            var options = new ArchiveOptions(model.Password, model.DictFileName);
            var token = app.ShowProgress("解压中...");
            await Task.Factory.StartNew(() => {
                var watch = new Stopwatch();
                watch.Start();
                using var reader = new UnityBundleScheme().Load(fileItems, options);
                try
                {
                    reader?.ExtractTo(model.FileName, token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                watch.Stop();
                Debug.WriteLine($"Use Time: {watch.Elapsed.TotalSeconds}");
                app.CloseProgress();
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
    }
}
