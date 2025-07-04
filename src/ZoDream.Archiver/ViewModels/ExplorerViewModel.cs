﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using ZoDream.Archiver.Controls;
using ZoDream.Archiver.Dialogs;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Media;

namespace ZoDream.Archiver.ViewModels
{
    public class ExplorerViewModel: ObservableObject
    {
        public ExplorerViewModel()
        {
            AddCommand = UICommand.Add(TapAdd);
            AddFolderCommand = UICommand.AddFolder(TapAddFolder);
            DeleteCommand = UICommand.Delete(TapDelete);
            SaveCommand = UICommand.Save(TapSaveAs);
            ViewCommand = UICommand.View<object>(TapView);
            BackCommand = UICommand.Backward(TapBack);
            SettingCommand = UICommand.Setting(TapSetting);
            DragCommand = new RelayCommand<IEnumerable<IStorageItem>>(TapDrag);
            _service = _app.Service;
            _storage = new StorageExplorer(_service);
            LoadSetting();
        }

        private readonly AppViewModel _app = App.ViewModel;
        private readonly List<ISourceEntry> _routeItems = [];
        private readonly IEntryService _service;
        private readonly StorageExplorer _storage;
        private IEntryExplorer? _explorer;

        public IEntryExplorer CurrentSource 
        {
            get => _explorer is null ? _storage : _explorer;
            set {
                _explorer?.Dispose();
                _explorer = value;
            }
        }

        private ObservableCollection<ISourceEntry> _fileItems = [];

        public ObservableCollection<ISourceEntry> FileItems {
            get => _fileItems;
            set => SetProperty(ref _fileItems, value);
        }

        private EntryViewModel? _selectedItem;

        public EntryViewModel? SelectedItem {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public bool CanGoBack => _routeItems.Count > 0;


        public ICommand AddCommand { get; private set; }
        public ICommand AddFolderCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DragCommand { get; private set; }
        public ICommand ViewCommand { get; private set; }
        public ICommand BackCommand { get; private set; }

        public ICommand SettingCommand { get; private set; }

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
        }

        private void TapBack()
        {
            if (!CanGoBack)
            {
                return;
            }
            var last = _routeItems.Count - 1;
            var item = _routeItems[last];
            _routeItems.RemoveAt(last);
            Open(item);
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
            _storage.Add(items.Select(i => i.Path));
            if (CanGoBack)
            {
                return;
            }
            Open(DirectoryEntry.Empty);
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
            Open(DirectoryEntry.Empty);
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
            _storage.Add(folder.Path);
            if (CanGoBack)
            {
                return;
            }
            Open(DirectoryEntry.Empty);
        }

        private void AddRoute(ISourceEntry entry)
        {
            if (_routeItems.Count > 0 && _routeItems.Last().FullPath == entry.FullPath)
            {
                return;
            }
            _routeItems.Add(entry);
        }

        private async void Open(ISourceEntry entry)
        {
            var e = await CurrentSource.OpenAsync(entry);
            if (e is ArchiveEntryStream s)
            {
                CurrentSource = s.Archive;
                Open(DirectoryEntry.Empty);
                return;
            }
            if (e is DirectoryEntryStream items)
            {
                FileItems.Clear();
                foreach (var item in items.Items.OrderByDescending(i => i.IsDirectory))
                {
                    if (item is TopDirectoryEntry)
                    {
                        AddRoute(item);
                        // FileItems.Add(item);
                        continue;
                    }
                    FileItems.Add(new EntryViewModel(item));
                }
                if (!items.CanGoBack)
                {
                    _routeItems.Clear();
                }
                OnPropertyChanged(nameof(CanGoBack));
                return;
            }
            if (e is MediaEntryStream media)
            {
                var file = await _service.Get<ITemporaryStorage>()
                    .CreateFileAsync(DateTime.Now.Ticks + media.Name);
                using (var fs = await file.OpenWriteAsync())
                {
                    media.SaveAs(fs);
                }
                await file.LaunchAsync();
            }
        }

        private void TapView(object? e)
        {
            if (e is ISourceEntry entry)
            {
                Open(entry);
                return;
            }
            if (SelectedItem is not null)
            {
                Open(SelectedItem);
                return;
            }
        }

        private async void TapSaveAs()
        {
            if (FileItems.Count > 0)
            {
                return;
            }
            var app = App.ViewModel;
            var picker = new ExtractDialog();
            var model = picker.ViewModel;
            model.IsSelected = SelectedItem is not null ? Visibility.Visible : Visibility.Collapsed;
            var res = await app.OpenFormAsync(picker);
            if (!res)
            {
                return;
            }
            var entryItems = SelectedItem is not null && model.OnlySelected ? [SelectedItem] : FileItems;
            var token = app.OpenProgress("解压中...");
            await Task.Factory.StartNew(() => {
                foreach (var item in entryItems)
                {
                    CurrentSource.SaveAs(item, model.FileName, model.ExtractMode, token);
                }
                app.CloseProgress();
                app.Success("解压已完成！");
            }, token);
        }

        private void TapDelete()
        {
            if (SelectedItem is null || CanGoBack)
            {
                return;
            }
            _storage.Remove(SelectedItem.FullPath);
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
            _service.Dispose();
        }
    }
}
