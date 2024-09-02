﻿using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Archiver.Converters
{
    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return "0B";
            }
            return FormatSize((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static string FormatSize(long size)
        {
            var len = size.ToString().Length;
            if (len < 4)
            {
                return $"{size}B";
            }
            if (len < 7)
            {
                return Math.Round(System.Convert.ToDouble(size / 1024d), 2) + "KB";
            }
            if (len < 10)
            {
                return Math.Round(System.Convert.ToDouble(size / 1024d / 1024), 2) + "MB";
            }
            if (len < 13)
            {
                return Math.Round(System.Convert.ToDouble(size / 1024d / 1024 / 1024), 2) + "GB";
            }
            if (len < 16)
            {
                return Math.Round(System.Convert.ToDouble(size / 1024d / 1024 / 1024 / 1024), 2) + "TB";
            }
            return Math.Round(System.Convert.ToDouble(size / 1024d / 1024 / 1024 / 1024 / 1024), 2) + "PB";
        }

        
    }
}
