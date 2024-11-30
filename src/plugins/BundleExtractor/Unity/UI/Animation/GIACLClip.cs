using System;
using System.IO;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class GIACLClip : ACLClip
    {
        public uint m_CurveCount;
        public uint m_ConstCurveCount;

        public byte[] m_ClipData;
        public byte[] m_DatabaseData;

        public override bool IsSet => m_ClipData is not null && m_ClipData.Length > 0 && m_DatabaseData is not null && m_DatabaseData.Length > 0;
        public override uint CurveCount => m_CurveCount;

        public GIACLClip()
        {
            m_CurveCount = 0;
            m_ConstCurveCount = 0;
            m_ClipData = Array.Empty<byte>();
            m_DatabaseData = Array.Empty<byte>();
        }

        public override void Read(UIReader reader)
        {
            var aclTracksCount = (int)reader.ReadUInt64();
            var aclTracksOffset = reader.Position + reader.ReadInt64();
            var aclTracksCurveCount = reader.ReadUInt32();
            if (aclTracksOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = aclTracksOffset;

            var tracksBytes = reader.ReadBytes(aclTracksCount);
            reader.AlignStream();

            using var tracksMS = new MemoryStream();
            tracksMS.Write(tracksBytes);
            //tracksMS.AlignStream();
            m_CurveCount = aclTracksCurveCount;
            m_ClipData = tracksMS.ToArray();

            reader.Position = pos;

            var aclDatabaseCount = reader.ReadInt32();
            var aclDatabaseOffset = reader.Position + reader.ReadInt64();
            var aclDatabaseCurveCount = (uint)reader.ReadUInt64();
            if (aclDatabaseOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            pos = reader.Position;
            reader.Position = aclDatabaseOffset;

            var databaseBytes = reader.ReadBytes(aclDatabaseCount);
            reader.AlignStream();

            using var databaseMS = new MemoryStream();
            databaseMS.Write(databaseBytes);
            //databaseMS.AlignStream();

            m_ConstCurveCount = aclDatabaseCurveCount;
            m_DatabaseData = databaseMS.ToArray();

            reader.Position = pos;
        }
    }

}
