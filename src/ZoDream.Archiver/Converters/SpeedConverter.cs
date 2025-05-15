using Microsoft.UI.Xaml.Data;
using System;
using ZoDream.Archiver.ViewModels;
using ZoDream.Shared.Net;

namespace ZoDream.Archiver.Converters
{
    public class SpeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DownloadItemViewModel d)
            {
                if (d.Status == RequestStatus.Paused)
                {
                    return $"{SizeConverter.FormatSize(d.Value)}/{SizeConverter.FormatSize(d.Length)}, {StatusConverter.Format(d.Status)}";
                }
                return $"{SizeConverter.FormatSize(d.Speed)}/s - {SizeConverter.FormatSize(d.Value)}/{SizeConverter.FormatSize(d.Length)}, 剩余时间 {TimeConverter.FormatHour(d.ElapsedTime)}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
