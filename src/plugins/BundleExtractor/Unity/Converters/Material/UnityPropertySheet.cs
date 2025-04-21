using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class PropertySheetConverter : BundleConverter<PropertySheet>
    {
        public override PropertySheet? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new PropertySheet();
            res.TexEnvs = reader.ReadArray(_ => {
                var key = reader.ReadAlignedString();
                var value = serializer.Deserialize<TexEnv>(reader);
                return new KeyValuePair<string, TexEnv>(key, value);
            });
            if (version.GreaterThanOrEquals(2021, 1)) //2021.1 and up
            {
                res.Ints = reader.ReadArray(_ => {
                    return new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32());
                });
            }

            res.Floats = reader.ReadArray(_ => {
                return new KeyValuePair<string, float>(reader.ReadAlignedString(), reader.ReadSingle());
            });

            res.Colors = reader.ReadArray(_ => {
                return new KeyValuePair<string, Vector4>(reader.ReadAlignedString(), reader.ReadVector4());
            });
            return res;
        }
    }

}
