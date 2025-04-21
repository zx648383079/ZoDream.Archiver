using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class UAVParameterConverter : BundleConverter<UAVParameter>
    {
        public override UAVParameter Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new UAVParameter
            {
                NameIndex = reader.ReadInt32(),
                Index = reader.ReadInt32(),
                OriginalIndex = reader.ReadInt32()
            };
            return res;
        }
    }

}
