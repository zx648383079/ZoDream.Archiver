using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class LnDACLClip : ACLClip
    {
        public uint m_CurveCount;
        public byte[] m_ClipData;

        public override bool IsSet => m_ClipData is not null && m_ClipData.Length > 0;
        public override uint CurveCount => m_CurveCount;
        public override void Read(UIReader reader)
        {
            m_CurveCount = reader.ReadUInt32();
            var compressedTransformTracksSize = reader.ReadUInt32();
            var compressedScalarTracksSize = reader.ReadUInt32();
            var aclTransformCount = reader.ReadUInt32();
            var aclScalarCount = reader.ReadUInt32();

            var compressedTransformTracksCount = reader.ReadInt32() * 0x10;
            var compressedTransformTracks = reader.ReadBytes(compressedTransformTracksCount);
            var compressedScalarTracksCount = reader.ReadInt32() * 0x10;
            var compressedScalarTracks = reader.ReadBytes(compressedScalarTracksCount);

            int numaclTransformTrackIDToBindingCurveID = reader.ReadInt32();
            var aclTransformTrackIDToBindingCurveID = new List<AclTransformTrackIDToBindingCurveID>();
            for (int i = 0; i < numaclTransformTrackIDToBindingCurveID; i++)
            {
                aclTransformTrackIDToBindingCurveID.Add(new AclTransformTrackIDToBindingCurveID(reader));
            }
            var aclScalarTrackIDToBindingCurveID = reader.ReadArray(r => r.ReadUInt32());
        }
    }

}
