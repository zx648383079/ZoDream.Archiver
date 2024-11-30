using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class StreamedClip
    {
        public uint[] data;
        public uint curveCount;
        public StreamedClip() { }

        public StreamedClip(UIReader reader)
        {
            data = reader.ReadArray(r => r.ReadUInt32());
            curveCount = reader.ReadUInt32();
        }
        public static StreamedClip ParseGI(UIReader reader)
        {
            var streamedClipCount = (int)reader.ReadUInt64();
            var streamedClipOffset = reader.Position + reader.ReadInt64();
            var streamedClipCurveCount = (uint)reader.ReadUInt64();
            if (streamedClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = streamedClipOffset;

            var streamedClip = new StreamedClip()
            {
                data = reader.ReadArray(streamedClipCount, r => r.ReadUInt32()),
                curveCount = streamedClipCurveCount
            };

            reader.Position = pos;

            return streamedClip;
        }

        public class StreamedCurveKey
        {
            public int index;
            public float[] coeff;

            public float value;
            public float outSlope;
            public float inSlope;

            public StreamedCurveKey(EndianReader reader)
            {
                index = reader.ReadInt32();
                coeff = reader.ReadArray(4, r => r.ReadSingle());

                outSlope = coeff[2];
                value = coeff[3];
            }

            public float CalculateNextInSlope(float dx, StreamedCurveKey rhs)
            {
                //Stepped
                if (coeff[0] == 0f && coeff[1] == 0f && coeff[2] == 0f)
                {
                    return float.PositiveInfinity;
                }

                dx = Math.Max(dx, 0.0001f);
                var dy = rhs.value - value;
                var length = 1.0f / (dx * dx);
                var d1 = outSlope * dx;
                var d2 = dy + dy + dy - d1 - d1 - coeff[1] / length;
                return d2 / dx;
            }
        }

        public class StreamedFrame
        {
            public float time;
            public List<StreamedCurveKey> keyList;

            public StreamedFrame(EndianReader reader)
            {
                time = reader.ReadSingle();

                int numKeys = reader.ReadInt32();
                keyList = new List<StreamedCurveKey>();
                for (int i = 0; i < numKeys; i++)
                {
                    keyList.Add(new StreamedCurveKey(reader));
                }
            }
        }

        public List<StreamedFrame> ReadData()
        {
            var frameList = new List<StreamedFrame>();
            var buffer = new byte[data.Length * 4];
            Buffer.BlockCopy(data, 0, buffer, 0, buffer.Length);
            using (var reader = new EndianReader(new MemoryStream(buffer), EndianType.LittleEndian))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    frameList.Add(new StreamedFrame(reader));
                }
            }

            for (int frameIndex = 2; frameIndex < frameList.Count - 1; frameIndex++)
            {
                var frame = frameList[frameIndex];
                foreach (var curveKey in frame.keyList)
                {
                    for (int i = frameIndex - 1; i >= 0; i--)
                    {
                        var preFrame = frameList[i];
                        var preCurveKey = preFrame.keyList.FirstOrDefault(x => x.index == curveKey.index);
                        if (preCurveKey != null)
                        {
                            curveKey.inSlope = preCurveKey.CalculateNextInSlope(frame.time - preFrame.time, curveKey);
                            break;
                        }
                    }
                }
            }
            return frameList;
        }
    }

}
