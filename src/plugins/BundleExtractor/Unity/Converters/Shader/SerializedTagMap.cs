using System;
using System.Collections.Generic;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedTagMapConverter : BundleConverter<SerializedTagMap>
    {
        public override SerializedTagMap? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Tags = reader.ReadArray(_ => new KeyValuePair<string, string>(reader.ReadAlignedString(), reader.ReadAlignedString()))
            };
        }
    }

}
