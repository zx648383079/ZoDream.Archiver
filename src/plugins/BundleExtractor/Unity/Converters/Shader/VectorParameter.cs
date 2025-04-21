using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class VectorParameterConverter : BundleConverter<VectorParameter>
    {
        public override VectorParameter Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new VectorParameter();
            ReadBase(ref res, reader, serializer, () => { });
            return res;
        }

        public void ReadBase(ref VectorParameter res, IBundleBinaryReader reader, IBundleSerializer serializer, Action cb)
        {
            res.NameIndex = reader.ReadInt32();
            res.Index = reader.ReadInt32();
            res.ArraySize = reader.ReadInt32();
            cb.Invoke();
            res.Type = reader.ReadSByte();
            res.Dim = reader.ReadSByte();
            reader.AlignStream();
        }
    }

}
