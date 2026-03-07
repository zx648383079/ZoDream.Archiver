using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{

    internal class FloatCurveConverter : BundleConverter<FloatCurve>
    {
        public override FloatCurve Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new FloatCurve();
            res.Curve.PreInfinity = reader.ReadInt32();
            res.Curve.PostInfinity = reader.ReadInt32();
            res.Curve.Curve = reader.ReadArray(_ => {
                var position = reader.ReadSingle();
                var value = reader.ReadSingle();
                var tangentIn = reader.ReadSingle();
                var tangentOut = reader.ReadSingle();
                var continuity = reader.ReadInt32();
                return new Keyframe<float>()
                {
                    Time = position,
                    InSlope = tangentIn,
                    Value = value,
                    OutSlope = tangentOut,
                    WeightedMode = continuity
                };
            });

            return res;
        }
    }

    internal class VideoConverter : BundleConverter<VideoClip>
    {
        public override VideoClip Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var fileName = XnbReader.ReadString(reader);
            var duration = reader.ReadInt32();
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var framesPerSecond = reader.ReadSingle();
            int videoSoundtrackType = reader.ReadInt32();

            return new VideoClip();
        }
    }
}
