using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Object = UnityEngine.Object;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class GenericBindingConverter : BundleConverter<GenericBinding>
    {
        public override GenericBinding? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new GenericBinding();
            var version = reader.Get<Version>();
            res.Path = reader.ReadUInt32();
            res.Attribute = reader.ReadUInt32();
            res.Script = reader.ReadPPtr<Object>(serializer);
            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                res.TypeID = (NativeClassID)reader.ReadInt32();
            }
            else
            {
                res.TypeID = (NativeClassID)reader.ReadUInt16();
            }
            res.CustomType = reader.ReadByte();
            res.IsPPtrCurve = reader.ReadByte();
            if (version.GreaterThanOrEquals(2022, 1)) //2022.1 and up
            {
                res.IsIntCurve = reader.ReadByte();
            }
            reader.AlignStream();
            return res;
        }

    }
}
