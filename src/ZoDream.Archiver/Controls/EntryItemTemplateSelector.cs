using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using ZoDream.Archiver.ViewModels;

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
            return base.SelectTemplateCore(item, container);
        }
    }
}
