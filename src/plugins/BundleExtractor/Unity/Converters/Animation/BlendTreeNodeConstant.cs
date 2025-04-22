using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class BlendTreeNodeConstantConverter : BundleConverter<BlendTreeNodeConstant>
    {
        public override BlendTreeNodeConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new BlendTreeNodeConstant();
            ReadBase(res, reader, serializer, () => { });
            var version = reader.Get<Version>();
            if (version.GreaterThanOrEquals(4, 1, 3)) //4.1.3 and up
            {
                res.CycleOffset = reader.ReadSingle();
                res.Mirror = reader.ReadBoolean();
                reader.AlignStream();
            }
            return res;
        }

        public static void ReadBase(BlendTreeNodeConstant res, IBundleBinaryReader reader, 
            IBundleSerializer serializer, Action cb)
        {
            var version = reader.Get<Version>();

            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                res.BlendType = reader.ReadUInt32();
            }
            res.BlendEventID = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                res.BlendEventYID = reader.ReadUInt32();
            }
            res.ChildIndices = reader.ReadArray(r => r.ReadUInt32());
            if (version.LessThan(4, 1)) //4.1 down
            {
                res.ChildThresholdArray = reader.ReadArray(r => r.ReadSingle());
            }

            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                res.Blend1dData = serializer.Deserialize<Blend1dDataConstant>(reader);
                res.Blend2dData = serializer.Deserialize<Blend2dDataConstant>(reader);
            }

            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                res.BlendDirectData = serializer.Deserialize<BlendDirectDataConstant>(reader);
            }

            res.ClipID = reader.ReadUInt32();
            if (version.Major == 4 && version.Minor >= 5) //4.5 - 5.0
            {
                res.ClipIndex = reader.ReadUInt32();
            }

            res.Duration = reader.ReadSingle();

            cb.Invoke();
        }
    }
}
