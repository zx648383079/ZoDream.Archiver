using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class HandleConverter : BundleConverter<Handle>
    {
        public override Handle? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                X = reader.ReadXForm(),
                ParentHumanIndex = reader.ReadUInt32(),
                ID = reader.ReadUInt32()
            };
        }
    }

}
