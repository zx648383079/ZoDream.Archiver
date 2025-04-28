using System;
using System.Collections.Generic;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedPassConverter : BundleConverter<SerializedPass>
    {
        public override SerializedPass? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new SerializedPass();
            if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                res.EditorDataHash = reader.ReadArray<Hash128>(serializer);
          
                reader.AlignStream();
                res.Platforms = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
                if (version.LessThan(2021, 1)) //2021.1 and down
                {
                    res.LocalKeywordMask = reader.ReadArray(r => r.ReadUInt16());
                    reader.AlignStream();
                    res.GlobalKeywordMask = reader.ReadArray(r => r.ReadUInt16());
                    reader.AlignStream();
                }
            }

            res.NameIndices = reader.ReadArray(_ => new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32()));


            res.Type = (PassType)reader.ReadInt32();
            res.State = serializer.Deserialize<SerializedShaderState>(reader);
            res.ProgramMask = reader.ReadUInt32();
            res.ProgVertex = serializer.Deserialize<SerializedProgram>(reader);
            res.ProgFragment = serializer.Deserialize<SerializedProgram>(reader);
            res.ProgGeometry = serializer.Deserialize<SerializedProgram>(reader);
            res.ProgHull = serializer.Deserialize<SerializedProgram>(reader);
            res.ProgDomain = serializer.Deserialize<SerializedProgram>(reader);
            if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
            {
                res.ProgRayTracing = serializer.Deserialize<SerializedProgram>(reader);
            }
            res.HasInstancingVariant = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(2018)) //2018 and up
            {
                var m_HasProceduralInstancingVariant = reader.ReadBoolean();
            }
            reader.AlignStream();
            res.UseName = reader.ReadAlignedString();
            res.Name = reader.ReadAlignedString();
            res.TextureName = reader.ReadAlignedString();
            res.Tags = serializer.Deserialize<SerializedTagMap>(reader);
            if (version.Major == 2021 && version.Minor >= 2) //2021.2 ~2021.x
            {
                res.SerializedKeywordStateMask = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
            }
            return res;
        }
    }

}
