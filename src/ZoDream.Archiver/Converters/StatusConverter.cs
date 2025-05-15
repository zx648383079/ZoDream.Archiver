using Microsoft.UI.Xaml.Data;
using System;
using ZoDream.Shared.Net;

namespace ZoDream.Archiver.Converters
{
    public class StatusConverter : IValueConverter
    {
    

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is RequestStatus request)
            {
                return Format(request);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static string Format(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Waiting => "Waiting",
                RequestStatus.Sending => "Sending",
                RequestStatus.Receiving => "Receiving",
                RequestStatus.Finished => "Finished",
                RequestStatus.Paused => "Paused",
                RequestStatus.Canceled => "Canceled",
                RequestStatus.Occurred => "Occurred",
                _ => string.Empty,
            };
        }
    }
}
