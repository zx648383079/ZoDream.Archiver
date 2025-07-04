﻿using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Egret
{
    internal class FrameSheetFile
    {
        public string File { get; set; } = string.Empty;

        public IDictionary<string, FrameItem>? Frames { get; set; }
    }

    internal class FrameItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public int OffX { get; set; }
        public int OffY { get; set; }
        public int SourceH { get; set; }
        public int SourceW { get; set; }
    }
}
