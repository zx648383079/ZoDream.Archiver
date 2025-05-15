using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoDream.Archiver.ViewModels;
using ZoDream.Shared.Net;

namespace ZoDream.Archiver.Controls
{
    public class DownloadItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate TransferTemplate { get; set; }
        public DataTemplate FinishedTemplate { get; set; }
        public DataTemplate PausedTemplate { get; set; }
        public DataTemplate CanceledTemplate { get; set; }
        public DataTemplate OccurredTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is DownloadItemViewModel o)
            {
                return o.Status switch
                {
                    RequestStatus.Sending or RequestStatus.Receiving => TransferTemplate,
                    RequestStatus.Finished => FinishedTemplate,
                    RequestStatus.Paused => PausedTemplate,
                    RequestStatus.Canceled => CanceledTemplate,
                    RequestStatus.Occurred => OccurredTemplate,
                    _ => DefaultTemplate
                };
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
