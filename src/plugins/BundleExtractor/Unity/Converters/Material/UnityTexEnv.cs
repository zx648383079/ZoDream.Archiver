using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class TexEnvConverter : BundleConverter<TexEnv>
    {
        public override TexEnv? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new TexEnv
            {
                Texture = serializer.Deserialize<PPtr<Texture>>(reader),
                Scale = reader.ReadVector2(),
                Offset = reader.ReadVector2()
            };
            return res;
        }
    }

}
