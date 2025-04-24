using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class MatrixParameterConverter : BundleConverter<MatrixParameter>
    {
        public override MatrixParameter Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new MatrixParameter();
            ReadBase(ref res, reader, serializer, () => { });
            return res;
        }

        public static void ReadBase(ref MatrixParameter res, IBundleBinaryReader reader, IBundleSerializer serializer, Action cb)
        {
            res.NameIndex = reader.ReadInt32();
            res.Index = reader.ReadInt32();
            res.ArraySize = reader.ReadInt32();
            cb.Invoke();

            res.Type = reader.ReadSByte();
            res.RowCount = reader.ReadSByte();
            reader.AlignStream();
        }
    }

}
