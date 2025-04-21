using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class StreamedClipConverter : BundleConverter<StreamedClip>
    {
        public override StreamedClip Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Data = reader.ReadArray(r => r.ReadUInt32()),
                CurveCount = reader.ReadUInt32()
            };
        }

        

        public List<StreamedFrame> ReadData()
        {
            var frameList = new List<StreamedFrame>();
            var buffer = new byte[data.Length * 4];
            Buffer.BlockCopy(data, 0, buffer, 0, buffer.Length);
            using (var reader = new BundleBinaryReader(new MemoryStream(buffer), EndianType.LittleEndian))
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
    internal class StreamedCurveKeyConverter : BundleConverter<StreamedCurveKey>
    {
        public override StreamedCurveKey Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new StreamedCurveKey();
            res.Index = reader.ReadInt32();
            res.Coeff = reader.ReadArray(4, (r, _) => r.ReadSingle());

            res.OutSlope = res.Coeff[2];
            res.Value = res.Coeff[3];
            return res;
        }

        public float CalculateNextInSlope(StreamedCurveKey res, float dx, StreamedCurveKey rhs)
        {
            //Stepped
            if (res.Coeff[0] == 0f && res.Coeff[1] == 0f && res.Coeff[2] == 0f)
            {
                return float.PositiveInfinity;
            }

            dx = Math.Max(dx, 0.0001f);
            var dy = rhs.Value - res.Value;
            var length = 1.0f / (dx * dx);
            var d1 = res.OutSlope * dx;
            var d2 = dy + dy + dy - d1 - d1 - res.Coeff[1] / length;
            return d2 / dx;
        }
    }

    public class StreamedFrameConverter : BundleConverter<StreamedFrame>
    {
        public override StreamedFrame Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new StreamedFrame
            {
                Time = reader.ReadSingle(),

                KeyList = reader.ReadArray<StreamedCurveKey>(serializer)
            };
            return res;
        }
    }
}
