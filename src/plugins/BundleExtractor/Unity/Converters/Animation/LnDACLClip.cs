using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class LnDACLClipConverter : BundleConverter<LnDACLClip>
    {
        public override LnDACLClip? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new LnDACLClip
            {
                CurveCount = reader.ReadUInt32()
            };
            var compressedTransformTracksSize = reader.ReadUInt32();
            var compressedScalarTracksSize = reader.ReadUInt32();
            var aclTransformCount = reader.ReadUInt32();
            var aclScalarCount = reader.ReadUInt32();

            var compressedTransformTracksCount = reader.ReadInt32() * 0x10;
            var compressedTransformTracks = reader.ReadBytes(compressedTransformTracksCount);
            var compressedScalarTracksCount = reader.ReadInt32() * 0x10;
            var compressedScalarTracks = reader.ReadBytes(compressedScalarTracksCount);

            int numaclTransformTrackIDToBindingCurveID = reader.ReadInt32();
            var aclTransformTrackIDToBindingCurveID = reader.ReadArray<AclTransformTrackIDToBindingCurveID>(serializer);
            var aclScalarTrackIDToBindingCurveID = reader.ReadArray(r => r.ReadUInt32());
            return res;
        }
    }

}
