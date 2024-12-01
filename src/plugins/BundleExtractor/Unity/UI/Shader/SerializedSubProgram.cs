using System;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedSubProgram: IElementLoader
    {
        public uint m_BlobIndex;
        public ParserBindChannels m_Channels;
        public ushort[] m_KeywordIndices;
        public sbyte m_ShaderHardwareTier;
        public ShaderGpuProgramType m_GpuProgramType;
        public SerializedProgramParameters m_Parameters;
        public List<VectorParameter> m_VectorParams;
        public List<MatrixParameter> m_MatrixParams;
        public List<TextureParameter> m_TextureParams;
        public List<BufferBinding> m_BufferParams;
        public List<ConstantBuffer> m_ConstantBuffers;
        public List<BufferBinding> m_ConstantBufferBindings;
        public List<UAVParameter> m_UAVParams;
        public List<SamplerParameter> m_Samplers;

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

        public void Read(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            m_BlobIndex = reader.ReadUInt32();
            if (HasIsAdditionalBlob(reader.Get<SerializedType>()))
            {
                var m_IsAdditionalBlob = reader.ReadBoolean();
                reader.AlignStream();
            }
            m_Channels = new ParserBindChannels(reader);

            if (version.GreaterThanOrEquals(2019) && version.LessThan(2021, 1) || HasGlobalLocalKeywordIndices(reader.Get<SerializedType>())) //2019 ~2021.1
            {
                var m_GlobalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
                var m_LocalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
            }
            else
            {
                m_KeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                if (version.GreaterThanOrEquals(2017)) //2017 and up
                {
                    reader.AlignStream();
                }
            }

            m_ShaderHardwareTier = reader.ReadSByte();
            m_GpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
            reader.AlignStream();

            ReadBase(reader);
        }
        public void ReadBase(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            if (version.GreaterThanOrEquals(2020, 3, 2, UnityVersionType.Final, 1) || //2020.3.2f1 and up
              version.GreaterThanOrEquals(2021, 1, 4, UnityVersionType.Final, 1)) //2021.1.4f1 and up
            {
                m_Parameters = new SerializedProgramParameters(reader);
            }
            else
            {
                int numVectorParams = reader.ReadInt32();
                m_VectorParams = new List<VectorParameter>();
                for (int i = 0; i < numVectorParams; i++)
                {
                    m_VectorParams.Add(new VectorParameter(reader));
                }

                int numMatrixParams = reader.ReadInt32();
                m_MatrixParams = new List<MatrixParameter>();
                for (int i = 0; i < numMatrixParams; i++)
                {
                    m_MatrixParams.Add(new MatrixParameter(reader));
                }

                int numTextureParams = reader.ReadInt32();
                m_TextureParams = new List<TextureParameter>();
                for (int i = 0; i < numTextureParams; i++)
                {
                    m_TextureParams.Add(new TextureParameter(reader));
                }

                int numBufferParams = reader.ReadInt32();
                m_BufferParams = new List<BufferBinding>();
                for (int i = 0; i < numBufferParams; i++)
                {
                    m_BufferParams.Add(new BufferBinding(reader));
                }

                int numConstantBuffers = reader.ReadInt32();
                m_ConstantBuffers = new List<ConstantBuffer>();
                for (int i = 0; i < numConstantBuffers; i++)
                {
                    m_ConstantBuffers.Add(new ConstantBuffer(reader));
                }

                int numConstantBufferBindings = reader.ReadInt32();
                m_ConstantBufferBindings = new List<BufferBinding>();
                for (int i = 0; i < numConstantBufferBindings; i++)
                {
                    m_ConstantBufferBindings.Add(new BufferBinding(reader));
                }

                int numUAVParams = reader.ReadInt32();
                m_UAVParams = new List<UAVParameter>();
                for (int i = 0; i < numUAVParams; i++)
                {
                    m_UAVParams.Add(new UAVParameter(reader));
                }

                if (version.GreaterThanOrEquals(2017)) //2017 and up
                {
                    int numSamplers = reader.ReadInt32();
                    m_Samplers = new List<SamplerParameter>();
                    for (int i = 0; i < numSamplers; i++)
                    {
                        m_Samplers.Add(new SamplerParameter(reader));
                    }
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
                int numInstancedStructuredBuffers = reader.ReadInt32();
                var m_InstancedStructuredBuffers = new List<ConstantBuffer>();
                for (int i = 0; i < numInstancedStructuredBuffers; i++)
                {
                    m_InstancedStructuredBuffers.Add(new ConstantBuffer(reader));
                }
            }
        }
    }

}
