﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using ZoDream.Archiver.Controls;
using ZoDream.Archiver.Dialogs;
using ZoDream.Shared.Compression.Own;

namespace ZoDream.Archiver.ViewModels
{
    public class CompressViewModel : ObservableObject
    {
        public CompressViewModel()
        {
            AddCommand = UICommand.Add(TapAdd);
            AddFolderCommand = UICommand.AddFolder(TapAddFolder);
            DeleteCommand = UICommand.Delete(TapDelete);
            SaveCommand = UICommand.Save(TapSaveAs);
            DragCommand = new RelayCommand<IEnumerable<IStorageItem>>(TapDrag);
        }

        private ObservableCollection<EntryViewModel> _fileItems = [];

        public ObservableCollection<EntryViewModel> FileItems {
            get => _fileItems;
            set => SetProperty(ref _fileItems, value);
        }

        private EntryViewModel? _selectedItem;

        public EntryViewModel? SelectedItem {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }


        public ICommand AddCommand { get; private set; }
        public ICommand AddFolderCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DragCommand { get; private set; }

        private async void TapAdd()
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
                if (File.Exists(item))
                {
                    AddFile(item);
                    continue;
                }
                if (!Directory.Exists(item))
                {
                    continue;
                }
                foreach (var it in Directory.GetFiles(item, "*", SearchOption.AllDirectories))
                {
                    AddFile(Path.GetRelativePath(item, it), it);
                }
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
            App.ViewModel.InitializePicker(picker);
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
            var picker = new CompressDialog();
            var app = App.ViewModel;
            var res = await app.OpenDialogAsync(picker);
            if (res != ContentDialogResult.Primary)
            {
                return;
            }
            var model = picker.ViewModel;
            var token = app.OpenProgress();
            await Task.Factory.StartNew(() => {
                using var writer = model.Create();
                if (writer is null)
                {
                    return;
                }
                try
                {
                    if (writer is OwnDictionaryWriter d)
                    {
                        d.AddFile(FileItems.Select(i => i.FullPath).ToArray(),
                            app.UpdateProgress, token);
                        app.CloseProgress();
                        app.Success("压缩已完成！");
                        return;
                    }
                    var i = 0D;
                    foreach (var item in FileItems)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        writer.AddEntry(item.Name, item.FullPath);
                        app.UpdateProgress(++i / FileItems.Count, $"[{i}/{FileItems.Count}]{item.Name}...");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                app.CloseProgress();
                app.Success("压缩已完成！");
            }, token);
            
        }

        private void TapDelete()
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
