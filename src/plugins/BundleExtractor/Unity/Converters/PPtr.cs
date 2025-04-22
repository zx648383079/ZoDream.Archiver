using System;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class PPtrConverter : BundleConverter<PPtr>
    {
        public override PPtr Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new PPtr
            {
                FileID = reader.ReadInt32(),
                PathID = reader.Get<FormatVersion>() < FormatVersion.Unknown_14 ?
                    reader.ReadInt32() : reader.ReadInt64()
            };
            return res;
        }



    }
}
