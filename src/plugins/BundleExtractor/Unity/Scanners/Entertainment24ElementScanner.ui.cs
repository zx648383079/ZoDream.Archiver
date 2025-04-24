using System;
using System.Linq;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public partial class Entertainment24ElementScanner : BundleConverter
    {
        private readonly Type[] _includeItems = [typeof(Material)];
        public override bool CanConvert(Type objectType)
        {
            return _includeItems.Contains(objectType);
        }
        public override object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            if (objectType == typeof(Material))
            {
                return ReadMaterial(reader, serializer);
            }
            return null;
        }

        private Material ReadMaterial(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new Material();
            MaterialConverter.ReadBase(res, reader, serializer, () => { });
            var version = reader.Get<Version>();

            if (version.GreaterThanOrEquals(5, 1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }

            var value = reader.ReadInt32();

            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadArray(r => r.ReadString());
            }

            res.SavedProperties = serializer.Deserialize<PropertySheet>(reader);
            return res;
        }
    }
}
