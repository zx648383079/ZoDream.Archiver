using Microsoft.UI.Xaml;
using System;
using ZoDream.Archiver.ViewModels;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.Converters
{
    public static class ConverterHelper
    {
        public static string Format(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return "-";
            }
            return date.ToString("yyyy-MM-dd HH:mm");
        }

        public static string FormatIcon(IEntryItem model)
        {
            return IconConverter.Format(model);
        }

        public static string FormatSize(long size)
        {
            return SizeConverter.FormatSize(size);
        }

        public static string FormatSpeed(DownloadItemViewModel model)
        {
            if (model.Status == BundleStatus.Paused)
            {
                return $"{SizeConverter.FormatSize(model.Value)}/{SizeConverter.FormatSize(model.Length)}, {StatusConverter.Format(model.Status)}";
            }
            return $"{SizeConverter.FormatSize(model.Speed)}/s - {SizeConverter.FormatSize(model.Value)}/{SizeConverter.FormatSize(model.Length)}, 剩余时间 {TimeConverter.FormatHour(model.ElapsedTime)}";
        }

        public static string Format(BundleStatus status)
        {
            return StatusConverter.Format(status);
        }

        public static string FormatHour(int time)
        {
            return TimeConverter.FormatHour(time);
        }

        public static Visibility VisibleIf(bool val)
        {
            return val ? Visibility.Visible : Visibility.Collapsed;
        }
        public static Visibility CollapsedIf(bool val)
        {
            return VisibleIf(!val);
        }

        public static bool IsNormal(BundleStatus status)
        {
            return status is BundleStatus.None or BundleStatus.Waiting;
        }

        public static bool IsWorking(BundleStatus status)
        {
            return status is BundleStatus.Sending or BundleStatus.Receiving;
        }

        public static bool IsWorked(BundleStatus status)
        {
            return status is BundleStatus.Completed or BundleStatus.Cancelled or BundleStatus.Failed;
        }

        public static Visibility VisibleIfNormal(BundleStatus status)
        {
            return VisibleIf(IsNormal(status));
        }

        public static Visibility VisibleIfWorked(BundleStatus status)
        {
            return VisibleIf(IsWorked(status));
        }

        public static Visibility VisibleIfWorking(BundleStatus status)
        {
            return VisibleIf(IsWorking(status));
        }
        public static Visibility CollapsedIfWorking(BundleStatus status)
        {
            return CollapsedIf(IsWorking(status));
        }
    }
}
