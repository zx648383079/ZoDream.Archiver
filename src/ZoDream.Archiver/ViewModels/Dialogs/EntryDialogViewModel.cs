using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class EntryDialogViewModel : BindableBase
    {
        public EntryDialogViewModel()
        {
            SearchCommand = new RelayCommand<string>(TapSearch);
        }

        private IBundleEntrySource[] _items = [];

        private string _keywords = string.Empty;

        public string Keywords {
            get => _keywords;
            set => Set(ref _keywords, value);
        }

        private object[] _entryItems = [];

        public object[] EntryItems {
            get => _entryItems;
            set => Set(ref _entryItems, value);
        }

        public ICommand SearchCommand { get; private set; }

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
            var items = new List<object>();
            foreach (var item in _items)
            {
                var hasHeader = false;
                foreach (var it in item)
                {
                    if (!string.IsNullOrWhiteSpace(Keywords) && !it.Name.Contains(Keywords))
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
