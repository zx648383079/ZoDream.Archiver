using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class EffectMaterialConverter : BundleConverter<EffectMaterial>
    {
        public override EffectMaterial? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var source = XnbReader.ReadString(reader);
            var instance = reader.Get<XnbReader>();
            instance.ReadDictionary(reader, "System.String", "System.Object");
            return new EffectMaterial();
        }
    }
}
