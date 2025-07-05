using Microsoft.UI.Xaml.Data;
using System;
using ZoDream.Shared.Bundle;

namespace ZoDream.Archiver.Converters
{
    public class StatusConverter : IValueConverter
    {
    

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is BundleStatus request)
            {
                return Format(request);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static string Format(BundleStatus status)
        {
            return status switch
            {
                BundleStatus.Waiting => "Waiting",
                BundleStatus.Sending => "Sending",
                BundleStatus.Receiving => "Receiving",
                BundleStatus.Working => "Working",
                BundleStatus.Completed => "Completed",
                BundleStatus.Paused => "Paused",
                BundleStatus.Cancelled => "Cancelled",
                BundleStatus.Failed => "Occurred",
                _ => string.Empty,
            };
        }
    }
}
