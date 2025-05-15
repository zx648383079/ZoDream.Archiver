using Microsoft.UI.Xaml.Data;
using System;

namespace ZoDream.Archiver.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime d)
            {
                return d.ToString("yyyy-MM-dd HH:mm");
            }
            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
