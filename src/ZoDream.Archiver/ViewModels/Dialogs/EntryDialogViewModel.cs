using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using ZoDream.Shared.Bundle;

namespace ZoDream.Archiver.ViewModels
{
    public class EntryDialogViewModel : ObservableObject
    {
        public EntryDialogViewModel()
        {
            SearchCommand = new RelayCommand<string>(TapSearch);
            OnlyCommand = new RelayCommand(TapOnly);
            LinkFromCommand = new RelayCommand(TapLinkFrom);
            LinkToCommand = new RelayCommand(TapLinkTo);
            ViewFileCommand = new RelayCommand(TapViewFile);
            CopyIDCommand = new RelayCommand(TapCopyID);
            CopyPathCommand = new RelayCommand(TapCopyPath);
        }

        private IBundleEntrySource[] _items = [];

        private string _keywords = string.Empty;

        public string Keywords {
            get => _keywords;
            set => SetProperty(ref _keywords, value);
        }

        private object[] _entryItems = [];

        public object[] EntryItems {
            get => _entryItems;
            set => SetProperty(ref _entryItems, value);
        }

        private object? _selectedItem;

        public object? SelectedItem {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }


        public ICommand SearchCommand { get; private set; }
        public ICommand OnlyCommand { get; private set; }
        public ICommand LinkFromCommand { get; private set; }
        public ICommand LinkToCommand { get; private set; }
        public ICommand ViewFileCommand { get; private set; }
        public ICommand CopyPathCommand { get; private set; }
        public ICommand CopyIDCommand { get; private set; }
        private void TapOnly()
        {
            var arg = GetSource(SelectedItem);
            if (arg is null)
            {
                return;
            }
            Search(Keywords, [arg]);
        }
        /// <summary>
        /// 查看此文件引用
        /// </summary>
        private void TapLinkTo()
        {
            var arg = SelectedItem;
            if (arg is null)
            {
                EntryItems = [];
                return;
            }
            switch (arg)
            {
                case IBundleEntry e:
                    Search(string.Empty, GetLinkTo(e));
                    break;
                case IBundleEntrySource s:
                    Search(string.Empty, GetLinkTo(s));
                    break;
                default:
                    break;
            }
        }


        private IEnumerable<IBundleEntrySource> GetLinkTo(IBundleEntry arg)
        {
            foreach (var item in _items)
            {
                if (IsLinkTo(item, arg))
                {
                    yield return item;
                }
            }
        }

        private bool IsLinkTo(IBundleEntrySource source, IBundleEntry arg)
        {
            if (arg.Id > 0)
            {
                return ((BundleEntrySource)source).LinkedItems.Contains(arg.Id);
            }
            return ((BundleEntrySource)source).LinkedPartItems.Contains(arg.Name);
        }

        private bool IsLinkTo(IBundleEntrySource source, IBundleEntrySource arg)
        {
            foreach (var it in arg)
            {
                if (IsLinkTo(source, it))
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<IBundleEntrySource> GetLinkTo(IBundleEntrySource arg)
        {
            foreach (var item in _items)
            {
                if (IsLinkTo(item, arg))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<IBundleEntrySource> GetLinkFrom(IBundleEntrySource arg)
        {
            foreach (var item in _items)
            {
                if (IsLinkTo(arg, item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// 查看此文件依赖
        /// </summary>
        private void TapLinkFrom()
        {
            var arg = GetSource(SelectedItem);
            if (arg is null)
            {
                EntryItems = [];
                return;
            }
            Search(string.Empty, GetLinkFrom(arg));
        }

        

        private async void TapViewFile()
        {
            var arg = GetSource(SelectedItem);
            if (arg is null)
            {
                return;
            }
            await new Shared.IO.StorageFileEntry(arg.FullPath).LaunchAsync();
        }

        private IBundleEntrySource? GetSource(object? data)
        {
            if (data is null)
            {
                return null;
            }
            if (data is IBundleEntrySource s)
            {
                return s;
            }
            if (data is not IBundleEntry e)
            {
                return null;
            }
            foreach (var item in _items)
            {
                if (item.Contains(e))
                {
                    return item;
                }
            }
            return null;
        }

        private void TapCopyID()
        {
            var arg = SelectedItem;
            if (arg is null)
            {
                return;
            }
            if (arg is IBundleEntry e)
            {
                var package = new DataPackage();
                package.SetText(e.Id > 0 ? e.Id.ToString() : e.Name);
                Clipboard.SetContent(package);
            }
        }

        private void TapCopyPath()
        {
            var arg = SelectedItem;
            if (arg is null)
            {
                return;
            }
            if (arg is IBundleEntry e)
            {
                var package = new DataPackage();
                package.SetText(e.Name ?? e.Id.ToString());
                Clipboard.SetContent(package);
            } else if (arg is IBundleEntrySource s)
            {
                var package = new DataPackage();
                package.SetText(s.FullPath);
                Clipboard.SetContent(package);
            }
        }

        private void TapSearch(string? keywords)
        {
            Keywords = keywords ?? string.Empty;
            Refresh();
        }

        public void Load(string fileName)
        {
            using var fs = File.OpenRead(fileName);
            _items = [.. BundleStorage.LoadEntry(fs)];
            Refresh();
        }

        private void Refresh()
        {
            Search(Keywords);
        }


        private void Search(string keywords)
        {
            Search(keywords, _items);
        }

        private void Search(string keywords, IEnumerable<IBundleEntrySource> data)
        {
            var items = new List<object>();
            foreach (var item in data)
            {
                var hasHeader = false;
                foreach (var it in item)
                {
                    if (!string.IsNullOrWhiteSpace(keywords) &&
                        !it.Name.Contains(keywords, System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (!hasHeader)
                    {
                        items.Add(item);
                        hasHeader = true;
                    }
                    items.Add(it);
                }
            }
            EntryItems = [.. items];
        }
    }
}
