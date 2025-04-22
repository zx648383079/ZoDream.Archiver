using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class TexEnvConverter : BundleConverter<TexEnv>
    {
        public override TexEnv? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new TexEnv();
            ReadBase(res, reader, serializer, () => { });
            return res;
        }

        public static void ReadBase(TexEnv res, IBundleBinaryReader reader, IBundleSerializer serializer, Action cb)
        {
            res.Texture = reader.ReadPPtr<Texture>(serializer);
            res.Scale = reader.ReadVector2();
            res.Offset = reader.ReadVector2();
            cb.Invoke();
        }
    }
}
