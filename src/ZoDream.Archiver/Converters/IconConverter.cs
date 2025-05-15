using Microsoft.UI.Xaml.Data;
using System;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.Converters
{
    public class IconConverter : IValueConverter
    {
        

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
            {
                return b ? "\uE8B7" : "\uE8A5";
            }
            if (value is IEntryItem e)
            {
                return Format(e);
            }
            return "\uE8A5";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }

        public static string Format(IEntryItem model)
        {
            if (model.IsDirectory)
            {
                return "\uE8B7";
            }
            return model.Extension switch
            {
                ".png" or ".bmp" or ".jpeg" or ".jpg" or ".gif" or ".gif" => "\uE91B",
                ".mp3" or ".flac" => "\uE8D6",
                ".mp4" or ".avi" or ".mov" or ".wmv" or ".mpg" or ".flv" => "\uE8B2",
                ".zip" or ".rar" or ".7z" or ".gzip" or ".gz" or ".zipx" => "\uF012",
                ".exe" or ".dll" or ".apk" => "\uE7B8",
                _ => "\uE8A5",
            };
        }
    }
}
