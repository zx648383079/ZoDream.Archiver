using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.SpineExporter.Models
{
    public class DrawOrderTimeline: Timeline
    {
        public DrawOrderTimeline(int frameCount)
        {
            Frames = new float[frameCount];
            DrawOrders = new int[frameCount][];
        }
        public float[] Frames { get; set; } // time, ...
        public int[][] DrawOrders { get; set; }
        public int FrameCount => Frames.Length;

        public int PropertyId => ((int)TimelineType.DrawOrder << 24);
    }
}
