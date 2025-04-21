using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedProgramConverter : BundleConverter<SerializedProgram>
    {
        public override SerializedProgram? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SerializedProgram();
            var version = reader.Get<Version>();
            res.SubPrograms = reader.ReadArray(_ => serializer.Deserialize<SerializedSubProgram>(reader));
  

            if (version.GreaterThanOrEquals(2021, 3, 10, VersionType.Final, 1) || //2021.3.10f1 and up
               version.GreaterThanOrEquals(2022, 1, 13, VersionType.Final, 1)) //2022.1.13f1 and up
            {
                res.PlayerSubPrograms = reader.Read2DArray(_ => serializer.Deserialize<SerializedPlayerSubProgram>(reader));

                res.ParameterBlobIndices = reader.Read2DArray((r, _, _) => r.ReadUInt32());
            }

            if (version.GreaterThanOrEquals(2020, 3, 2, VersionType.Final, 1) || //2020.3.2f1 and up
               version.GreaterThanOrEquals(2021, 1, 1, VersionType.Final, 1)) //2021.1.1f1 and up
            {
                res.CommonParameters = serializer.Deserialize<SerializedProgramParameters>(reader);
            }

            if (version.GreaterThanOrEquals(2022, 1)) //2022.1 and up
            {
                res.SerializedKeywordStateMask = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
            }
            return res;
        }
    }

}
