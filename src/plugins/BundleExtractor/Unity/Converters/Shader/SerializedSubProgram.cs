using System;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedSubProgramConverter : BundleConverter<SerializedSubProgram>
    {
        public static bool HasGlobalLocalKeywordIndices(SerializedType type)
        {
            return Convert.ToHexString(type.OldTypeHash) switch
            {
                "E99740711222CD922E9A6F92FF1EB07A" or
                "450A058C218DAF000647948F2F59DA6D" or
                "B239746E4EC6E4D6D7BA27C84178610A" or
                "3FD560648A91A99210D5DDF2BE320536" => true,
                _ => false
            };
        }
        public static bool HasInstancedStructuredBuffers(SerializedType type)
        {
            return Convert.ToHexString(type.OldTypeHash) switch
            {
                "E99740711222CD922E9A6F92FF1EB07A" or
                "B239746E4EC6E4D6D7BA27C84178610A" or
                "3FD560648A91A99210D5DDF2BE320536" => true,
                _ => false
            };
        }
        public static bool HasIsAdditionalBlob(SerializedType type) => Convert.ToHexString(type.OldTypeHash) == "B239746E4EC6E4D6D7BA27C84178610A";

        public override SerializedSubProgram? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new SerializedSubProgram();
            res.BlobIndex = reader.ReadUInt32();
            if (HasIsAdditionalBlob(reader.Get<SerializedType>()))
            {
                var m_IsAdditionalBlob = reader.ReadBoolean();
                reader.AlignStream();
            }
            res.Channels = serializer.Deserialize<ParserBindChannels>(reader);

            if (version.GreaterThanOrEquals(2019) && version.LessThan(2021, 1) || HasGlobalLocalKeywordIndices(reader.Get<SerializedType>())) //2019 ~2021.1
            {
                var m_GlobalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
                var m_LocalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
            }
            else
            {
                res.KeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                if (version.GreaterThanOrEquals(2017)) //2017 and up
                {
                    reader.AlignStream();
                }
            }

            res.ShaderHardwareTier = reader.ReadSByte();
            res.GpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
            reader.AlignStream();

            ReadBase(res, reader, serializer, () => { });
            return res;
        }
        public void ReadBase(SerializedSubProgram res, IBundleBinaryReader reader, 
            IBundleSerializer serializer, Action cb)
        {
            var version = reader.Get<Version>();
            if (version.GreaterThanOrEquals(2020, 3, 2, VersionType.Final, 1) || //2020.3.2f1 and up
              version.GreaterThanOrEquals(2021, 1, 4, VersionType.Final, 1)) //2021.1.4f1 and up
            {
                res.Parameters = serializer.Deserialize<SerializedProgramParameters>(reader);
            }
            else
            {
                res.VectorParams = reader.ReadArray(_ => serializer.Deserialize<VectorParameter>(reader));
                res.MatrixParams = reader.ReadArray(_ => serializer.Deserialize<MatrixParameter>(reader));
                res.TextureParams = reader.ReadArray(_ => serializer.Deserialize<TextureParameter>(reader));
                res.BufferParams = reader.ReadArray(_ => serializer.Deserialize<BufferBinding>(reader));
                res.ConstantBuffers = reader.ReadArray(_ => serializer.Deserialize<ConstantBuffer>(reader));
                res.ConstantBufferBindings = reader.ReadArray(_ => serializer.Deserialize<BufferBinding>(reader));
                res.UAVParams = reader.ReadArray(_ => serializer.Deserialize< UAVParameter>(reader));
      
                if (version.GreaterThanOrEquals(2017)) //2017 and up
                {
                    res.Samplers = reader.ReadArray(_ => serializer.Deserialize<SamplerParameter>(reader));
                }
            }

            if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
            {
                if (version.GreaterThanOrEquals(2021)) //2021.1 and up
                {
                    var m_ShaderRequirements = reader.ReadInt64();
                }
                else
                {
                    var m_ShaderRequirements = reader.ReadInt32();
                }
            }

            if (HasInstancedStructuredBuffers(reader.Get<SerializedType>()))
            {
                var m_InstancedStructuredBuffers = reader.ReadArray(_ => serializer.Deserialize<ConstantBuffer>(reader));
            }
        }
    }

}
