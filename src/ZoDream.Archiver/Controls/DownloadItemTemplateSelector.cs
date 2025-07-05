using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoDream.Archiver.ViewModels;
using ZoDream.Shared.Bundle;

namespace ZoDream.Archiver.Controls
{
    public class DownloadItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate TransferTemplate { get; set; }
        public DataTemplate CompletedTemplate { get; set; }
        public DataTemplate PausedTemplate { get; set; }
        public DataTemplate CancelledTemplate { get; set; }
        public DataTemplate FailedTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is DownloadItemViewModel o)
            {
                return o.Status switch
                {
                    BundleStatus.Sending or BundleStatus.Receiving => TransferTemplate,
                    BundleStatus.Completed => CompletedTemplate,
                    BundleStatus.Paused => PausedTemplate,
                    BundleStatus.Cancelled => CancelledTemplate,
                    BundleStatus.Failed => FailedTemplate,
                    _ => DefaultTemplate
                };
            }
            return base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is DownloadItemViewModel o)
            {
                return SelectTemplateCore(o);
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
