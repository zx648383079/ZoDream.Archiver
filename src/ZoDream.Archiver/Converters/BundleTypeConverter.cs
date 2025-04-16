using Microsoft.UI.Xaml.Data;
using System;
using ZoDream.BundleExtractor;

namespace ZoDream.Archiver.Converters
{
    public class BundleTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return BundleScheme.FormatEntryType((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
