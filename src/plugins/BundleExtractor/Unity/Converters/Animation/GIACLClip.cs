using System;
using System.IO;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class GIACLClipConverter : BundleConverter<GIACLClip>
    {
        public override GIACLClip? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new GIACLClip();
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
            res.CurveCount = aclTracksCurveCount;
            res.ClipData = tracksMS.ToArray();

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

            res.ConstCurveCount = aclDatabaseCurveCount;
            res.DatabaseData = databaseMS.ToArray();

            reader.Position = pos;
            return res;
        }
    }

}
