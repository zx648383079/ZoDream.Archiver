using Microsoft.UI.Xaml.Data;
using System;
using ZoDream.Archiver.ViewModels;

namespace ZoDream.Archiver.Converters
{
    public class SpeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DownloadItemViewModel d)
            {
                return ConverterHelper.FormatSpeed(d);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
