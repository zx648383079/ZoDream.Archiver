using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class EntryDialogViewModel : BindableBase
    {
        public EntryDialogViewModel()
        {
            _source.Source = EntryItems;
        }

        private readonly CollectionViewSource _source = new() { IsSourceGrouped = true };

        public ICollectionView EntrySource => _source.View;

        private string _keywords = string.Empty;

        public string Keywords {
            get => _keywords;
            set => Set(ref _keywords, value);
        }

        private ObservableCollection<IBundleEntrySource> _entryItems = [];

        public ObservableCollection<IBundleEntrySource> EntryItems {
            get => _entryItems;
            set => Set(ref _entryItems, value);
        }

        public void Load(string fileName)
        {
            using var fs = File.OpenRead(fileName);
            EntryItems.Clear();
            foreach (var item in BundleStorage.LoadEntry(fs))
            {
                EntryItems.Add(item);
            }
        }
    }
}
