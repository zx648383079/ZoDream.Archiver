using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class StreamedClipConverter : BundleConverter<StreamedClip>
    {
        public override StreamedClip Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var length = reader.ReadInt32() * 4;
            return new()
            {
                Data = ReadData(reader, length, serializer),
                CurveCount = reader.ReadUInt32()
            };
        }

        

        public static StreamedFrame[] ReadData(IBundleBinaryReader reader, int length, IBundleSerializer serializer)
        {
            var frameList = new List<StreamedFrame>();
            var end = reader.Position + length;
            while (reader.Position < end)
            {
                frameList.Add(serializer.Deserialize<StreamedFrame>(reader));
            }
            reader.Position = end;
            for (int frameIndex = 2; frameIndex < frameList.Count - 1; frameIndex++)
            {
                var frame = frameList[frameIndex];
                foreach (var curveKey in frame.KeyList)
                {
                    for (int i = frameIndex - 1; i >= 0; i--)
                    {
                        var preFrame = frameList[i];
                        var preCurveKey = preFrame.KeyList.FirstOrDefault(x => x.Index == curveKey.Index);
                        if (preCurveKey != null)
                        {
                            curveKey.InSlope = StreamedCurveKeyConverter.CalculateNextInSlope(preCurveKey, frame.Time - preFrame.Time, curveKey);
                            break;
                        }
                    }
                }
            }
            return [..frameList];
        }
    }
    internal class StreamedCurveKeyConverter : BundleConverter<StreamedCurveKey>
    {
        public override StreamedCurveKey Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new StreamedCurveKey
            {
                Index = reader.ReadInt32(),
                Coeff = reader.ReadArray(4, (r, _) => r.ReadSingle())
            };

            res.OutSlope = res.Coeff[2];
            res.Value = res.Coeff[3];
            return res;
        }

        public static float CalculateNextInSlope(StreamedCurveKey res, float dx, StreamedCurveKey rhs)
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
