using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoDream.Archiver.ViewModels;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.Controls
{
    public class EntryItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? DefaultTemplate { get; set; }
        public DataTemplate? DirectoryTemplate { get; set; }
        public DataTemplate? BackTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is TopDirectoryEntry)
            {
                return BackTemplate;
            }
            if (item is EntryViewModel e)
            {
                return e.IsDirectory ? DirectoryTemplate : DefaultTemplate;
            }
            if (item is IBundleEntrySource)
            {
                return DirectoryTemplate;
            }
            if (item is IBundleEntry)
            {
                return DefaultTemplate;
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
