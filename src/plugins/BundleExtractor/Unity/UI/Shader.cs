using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class Hash128
    {
        public byte[] bytes;

        public Hash128(UIReader reader)
        {
            bytes = reader.Reader.ReadBytes(16);
        }
    }

    public class StructParameter
    {
        public List<MatrixParameter> m_MatrixParams;
        public List<VectorParameter> m_VectorParams;

        public StructParameter(UIReader reader)
        {
            var m_NameIndex = reader.Reader.ReadInt32();
            var m_Index = reader.Reader.ReadInt32();
            var m_ArraySize = reader.Reader.ReadInt32();
            var m_StructSize = reader.Reader.ReadInt32();

            int numVectorParams = reader.Reader.ReadInt32();
            m_VectorParams = new List<VectorParameter>();
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams.Add(new VectorParameter(reader));
            }

            int numMatrixParams = reader.Reader.ReadInt32();
            m_MatrixParams = new List<MatrixParameter>();
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams.Add(new MatrixParameter(reader));
            }
        }
    }

    public class SamplerParameter
    {
        public uint sampler;
        public int bindPoint;

        public SamplerParameter(UIReader reader)
        {
            sampler = reader.Reader.ReadUInt32();
            bindPoint = reader.Reader.ReadInt32();
        }
    }
    public enum TextureDimension
    {
        Unknown = -1,
        None = 0,
        Any = 1,
        Tex2D = 2,
        Tex3D = 3,
        Cube = 4,
        Tex2DArray = 5,
        CubeArray = 6
    };

    public class SerializedTextureProperty
    {
        public string m_DefaultName;
        public TextureDimension m_TexDim;

        public SerializedTextureProperty(UIReader reader)
        {
            m_DefaultName = reader.ReadAlignedString();
            m_TexDim = (TextureDimension)reader.Reader.ReadInt32();
        }
    }

    public enum SerializedPropertyType
    {
        Color = 0,
        Vector = 1,
        Float = 2,
        Range = 3,
        Texture = 4,
        Int = 5
    };

    [Flags]
    public enum SerializedPropertyFlag
    {
        HideInInspector = 1 << 0,
        PerRendererData = 1 << 1,
        NoScaleOffset = 1 << 2,
        Normal = 1 << 3,
        HDR = 1 << 4,
        Gamma = 1 << 5,
        NonModifiableTextureData = 1 << 6,
        MainTexture = 1 << 7,
        MainColor = 1 << 8,
    }

    public class SerializedProperty
    {
        public string m_Name;
        public string m_Description;
        public string[] m_Attributes;
        public SerializedPropertyType m_Type;
        public SerializedPropertyFlag m_Flags;
        public float[] m_DefValue;
        public SerializedTextureProperty m_DefTexture;

        public SerializedProperty(UIReader reader)
        {
            m_Name = reader.ReadAlignedString();
            m_Description = reader.ReadAlignedString();
            m_Attributes = reader.ReadArray(r => r.ReadString());
            m_Type = (SerializedPropertyType)reader.Reader.ReadInt32();
            m_Flags = (SerializedPropertyFlag)reader.Reader.ReadUInt32();
            m_DefValue = reader.ReadArray(4, r => r.ReadSingle());
            m_DefTexture = new SerializedTextureProperty(reader);
        }
    }

    public class SerializedProperties
    {
        public List<SerializedProperty> m_Props;

        public SerializedProperties(UIReader reader)
        {
            int numProps = reader.Reader.ReadInt32();
            m_Props = new List<SerializedProperty>();
            for (int i = 0; i < numProps; i++)
            {
                m_Props.Add(new SerializedProperty(reader));
            }
        }
    }

    public class SerializedShaderFloatValue
    {
        public float val;
        public string name;

        public SerializedShaderFloatValue(UIReader reader)
        {
            val = reader.Reader.ReadSingle();
            name = reader.ReadAlignedString();
        }
    }

    public class SerializedShaderRTBlendState
    {
        public SerializedShaderFloatValue srcBlend;
        public SerializedShaderFloatValue destBlend;
        public SerializedShaderFloatValue srcBlendAlpha;
        public SerializedShaderFloatValue destBlendAlpha;
        public SerializedShaderFloatValue blendOp;
        public SerializedShaderFloatValue blendOpAlpha;
        public SerializedShaderFloatValue colMask;

        public SerializedShaderRTBlendState(UIReader reader)
        {
            srcBlend = new SerializedShaderFloatValue(reader);
            destBlend = new SerializedShaderFloatValue(reader);
            srcBlendAlpha = new SerializedShaderFloatValue(reader);
            destBlendAlpha = new SerializedShaderFloatValue(reader);
            blendOp = new SerializedShaderFloatValue(reader);
            blendOpAlpha = new SerializedShaderFloatValue(reader);
            colMask = new SerializedShaderFloatValue(reader);
        }
    }

    public class SerializedStencilOp
    {
        public SerializedShaderFloatValue pass;
        public SerializedShaderFloatValue fail;
        public SerializedShaderFloatValue zFail;
        public SerializedShaderFloatValue comp;

        public SerializedStencilOp(UIReader reader)
        {
            pass = new SerializedShaderFloatValue(reader);
            fail = new SerializedShaderFloatValue(reader);
            zFail = new SerializedShaderFloatValue(reader);
            comp = new SerializedShaderFloatValue(reader);
        }
    }

    public class SerializedShaderVectorValue
    {
        public SerializedShaderFloatValue x;
        public SerializedShaderFloatValue y;
        public SerializedShaderFloatValue z;
        public SerializedShaderFloatValue w;
        public string name;

        public SerializedShaderVectorValue(UIReader reader)
        {
            x = new SerializedShaderFloatValue(reader);
            y = new SerializedShaderFloatValue(reader);
            z = new SerializedShaderFloatValue(reader);
            w = new SerializedShaderFloatValue(reader);
            name = reader.ReadAlignedString();
        }
    }

    public enum FogMode
    {
        Unknown = -1,
        Disabled = 0,
        Linear = 1,
        Exp = 2,
        Exp2 = 3
    };

    public class SerializedShaderState
    {
        public string m_Name;
        public List<SerializedShaderRTBlendState> rtBlend;
        public bool rtSeparateBlend;
        public SerializedShaderFloatValue zClip;
        public SerializedShaderFloatValue zTest;
        public SerializedShaderFloatValue zWrite;
        public SerializedShaderFloatValue culling;
        public SerializedShaderFloatValue conservative;
        public SerializedShaderFloatValue offsetFactor;
        public SerializedShaderFloatValue offsetUnits;
        public SerializedShaderFloatValue alphaToMask;
        public SerializedStencilOp stencilOp;
        public SerializedStencilOp stencilOpFront;
        public SerializedStencilOp stencilOpBack;
        public SerializedShaderFloatValue stencilReadMask;
        public SerializedShaderFloatValue stencilWriteMask;
        public SerializedShaderFloatValue stencilRef;
        public SerializedShaderFloatValue fogStart;
        public SerializedShaderFloatValue fogEnd;
        public SerializedShaderFloatValue fogDensity;
        public SerializedShaderVectorValue fogColor;
        public FogMode fogMode;
        public int gpuProgramID;
        public SerializedTagMap m_Tags;
        public int m_LOD;
        public bool lighting;

        public SerializedShaderState(UIReader reader)
        {
            var version = reader.Version;

            m_Name = reader.ReadAlignedString();
            rtBlend = new List<SerializedShaderRTBlendState>();
            for (int i = 0; i < 8; i++)
            {
                rtBlend.Add(new SerializedShaderRTBlendState(reader));
            }
            rtSeparateBlend = reader.Reader.ReadBoolean();
            reader.Reader.AlignStream();
            if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
            {
                zClip = new SerializedShaderFloatValue(reader);
            }
            zTest = new SerializedShaderFloatValue(reader);
            zWrite = new SerializedShaderFloatValue(reader);
            culling = new SerializedShaderFloatValue(reader);
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                conservative = new SerializedShaderFloatValue(reader);
            }
            offsetFactor = new SerializedShaderFloatValue(reader);
            offsetUnits = new SerializedShaderFloatValue(reader);
            alphaToMask = new SerializedShaderFloatValue(reader);
            stencilOp = new SerializedStencilOp(reader);
            stencilOpFront = new SerializedStencilOp(reader);
            stencilOpBack = new SerializedStencilOp(reader);
            stencilReadMask = new SerializedShaderFloatValue(reader);
            stencilWriteMask = new SerializedShaderFloatValue(reader);
            stencilRef = new SerializedShaderFloatValue(reader);
            fogStart = new SerializedShaderFloatValue(reader);
            fogEnd = new SerializedShaderFloatValue(reader);
            fogDensity = new SerializedShaderFloatValue(reader);
            fogColor = new SerializedShaderVectorValue(reader);
            fogMode = (FogMode)reader.Reader.ReadInt32();
            gpuProgramID = reader.Reader.ReadInt32();
            m_Tags = new SerializedTagMap(reader);
            m_LOD = reader.Reader.ReadInt32();
            if (reader.IsLoveAndDeepspace())
            {
                int numOverrideKeywordAndStage = reader.Reader.ReadInt32();
                var m_OverrideKeywordAndStage = new List<KeyValuePair<string, uint>>();
                for (int i = 0; i < numOverrideKeywordAndStage; i++)
                {
                    m_OverrideKeywordAndStage.Add(new KeyValuePair<string, uint>(reader.ReadAlignedString(), reader.Reader.ReadUInt32()));
                }
            }
            lighting = reader.Reader.ReadBoolean();
            reader.Reader.AlignStream();
        }
    }

    public class ShaderBindChannel
    {
        public sbyte source;
        public sbyte target;

        public ShaderBindChannel(UIReader reader)
        {
            source = reader.Reader.ReadSByte();
            target = reader.Reader.ReadSByte();
        }
    }

    public class ParserBindChannels
    {
        public List<ShaderBindChannel> m_Channels;
        public uint m_SourceMap;

        public ParserBindChannels(UIReader reader)
        {
            int numChannels = reader.Reader.ReadInt32();
            m_Channels = new List<ShaderBindChannel>();
            for (int i = 0; i < numChannels; i++)
            {
                m_Channels.Add(new ShaderBindChannel(reader));
            }
            reader.Reader.AlignStream();

            m_SourceMap = reader.Reader.ReadUInt32();
        }
    }

    public class VectorParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_ArraySize;
        public sbyte m_Type;
        public sbyte m_Dim;

        public VectorParameter(UIReader reader)
        {
            m_NameIndex = reader.Reader.ReadInt32();
            m_Index = reader.Reader.ReadInt32();
            m_ArraySize = reader.Reader.ReadInt32();
            m_Type = reader.Reader.ReadSByte();
            m_Dim = reader.Reader.ReadSByte();
            reader.Reader.AlignStream();
        }
    }

    public class MatrixParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_ArraySize;
        public sbyte m_Type;
        public sbyte m_RowCount;

        public MatrixParameter(UIReader reader)
        {
            m_NameIndex = reader.Reader.ReadInt32();
            m_Index = reader.Reader.ReadInt32();
            m_ArraySize = reader.Reader.ReadInt32();
            m_Type = reader.Reader.ReadSByte();
            m_RowCount = reader.Reader.ReadSByte();
            reader.Reader.AlignStream();
        }
    }

    public class TextureParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_SamplerIndex;
        public sbyte m_Dim;

        public TextureParameter(UIReader reader)
        {
            var version = reader.Version;

            m_NameIndex = reader.Reader.ReadInt32();
            m_Index = reader.Reader.ReadInt32();
            m_SamplerIndex = reader.Reader.ReadInt32();
            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                var m_MultiSampled = reader.Reader.ReadBoolean();
            }
            m_Dim = reader.Reader.ReadSByte();
            reader.Reader.AlignStream();
        }
    }

    public class BufferBinding
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_ArraySize;

        public BufferBinding(UIReader reader)
        {
            var version = reader.Version;

            m_NameIndex = reader.Reader.ReadInt32();
            m_Index = reader.Reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                m_ArraySize = reader.Reader.ReadInt32();
            }
        }
    }

    public class ConstantBuffer
    {
        public int m_NameIndex;
        public List<MatrixParameter> m_MatrixParams;
        public List<VectorParameter> m_VectorParams;
        public List<StructParameter> m_StructParams;
        public int m_Size;
        public bool m_IsPartialCB;

        public ConstantBuffer(UIReader reader)
        {
            var version = reader.Version;

            m_NameIndex = reader.Reader.ReadInt32();

            int numMatrixParams = reader.Reader.ReadInt32();
            m_MatrixParams = new List<MatrixParameter>();
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams.Add(new MatrixParameter(reader));
            }

            int numVectorParams = reader.Reader.ReadInt32();
            m_VectorParams = new List<VectorParameter>();
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams.Add(new VectorParameter(reader));
            }
            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                int numStructParams = reader.Reader.ReadInt32();
                m_StructParams = new List<StructParameter>();
                for (int i = 0; i < numStructParams; i++)
                {
                    m_StructParams.Add(new StructParameter(reader));
                }
            }
            m_Size = reader.Reader.ReadInt32();

            if (version.GreaterThanOrEquals(2020, 3, 2, UnityVersionType.Final, 1) || //2020.3.2f1 and up
              version.GreaterThanOrEquals(2021, 1, 4, UnityVersionType.Final, 1)) //2021.1.4f1 and up
            {
                m_IsPartialCB = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();
            }
        }
    }

    public class UAVParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_OriginalIndex;

        public UAVParameter(EndianReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_OriginalIndex = reader.ReadInt32();
        }
    }

    public enum ShaderGpuProgramType
    {
        Unknown = 0,
        GLLegacy = 1,
        GLES31AEP = 2,
        GLES31 = 3,
        GLES3 = 4,
        GLES = 5,
        GLCore32 = 6,
        GLCore41 = 7,
        GLCore43 = 8,
        DX9VertexSM20 = 9,
        DX9VertexSM30 = 10,
        DX9PixelSM20 = 11,
        DX9PixelSM30 = 12,
        DX10Level9Vertex = 13,
        DX10Level9Pixel = 14,
        DX11VertexSM40 = 15,
        DX11VertexSM50 = 16,
        DX11PixelSM40 = 17,
        DX11PixelSM50 = 18,
        DX11GeometrySM40 = 19,
        DX11GeometrySM50 = 20,
        DX11HullSM50 = 21,
        DX11DomainSM50 = 22,
        MetalVS = 23,
        MetalFS = 24,
        SPIRV = 25,
        ConsoleVS = 26,
        ConsoleFS = 27,
        ConsoleHS = 28,
        ConsoleDS = 29,
        ConsoleGS = 30,
        RayTracing = 31,
        PS5NGGC = 32
    };

    public class SerializedProgramParameters
    {
        public List<VectorParameter> m_VectorParams;
        public List<MatrixParameter> m_MatrixParams;
        public List<TextureParameter> m_TextureParams;
        public List<BufferBinding> m_BufferParams;
        public List<ConstantBuffer> m_ConstantBuffers;
        public List<BufferBinding> m_ConstantBufferBindings;
        public List<UAVParameter> m_UAVParams;
        public List<SamplerParameter> m_Samplers;

        public SerializedProgramParameters(UIReader reader)
        {
            int numVectorParams = reader.Reader.ReadInt32();
            m_VectorParams = new List<VectorParameter>();
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams.Add(new VectorParameter(reader));
            }

            int numMatrixParams = reader.Reader.ReadInt32();
            m_MatrixParams = new List<MatrixParameter>();
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams.Add(new MatrixParameter(reader));
            }

            int numTextureParams = reader.Reader.ReadInt32();
            m_TextureParams = new List<TextureParameter>();
            for (int i = 0; i < numTextureParams; i++)
            {
                m_TextureParams.Add(new TextureParameter(reader));
            }

            int numBufferParams = reader.Reader.ReadInt32();
            m_BufferParams = new List<BufferBinding>();
            for (int i = 0; i < numBufferParams; i++)
            {
                m_BufferParams.Add(new BufferBinding(reader));
            }

            int numConstantBuffers = reader.Reader.ReadInt32();
            m_ConstantBuffers = new List<ConstantBuffer>();
            for (int i = 0; i < numConstantBuffers; i++)
            {
                m_ConstantBuffers.Add(new ConstantBuffer(reader));
            }

            int numConstantBufferBindings = reader.Reader.ReadInt32();
            m_ConstantBufferBindings = new List<BufferBinding>();
            for (int i = 0; i < numConstantBufferBindings; i++)
            {
                m_ConstantBufferBindings.Add(new BufferBinding(reader));
            }

            int numUAVParams = reader.Reader.ReadInt32();
            m_UAVParams = new List<UAVParameter>();
            for (int i = 0; i < numUAVParams; i++)
            {
                m_UAVParams.Add(new UAVParameter(reader.Reader));
            }

            int numSamplers = reader.Reader.ReadInt32();
            m_Samplers = new List<SamplerParameter>();
            for (int i = 0; i < numSamplers; i++)
            {
                m_Samplers.Add(new SamplerParameter(reader));
            }
        }
    }

    public class SerializedSubProgram
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

        public SerializedSubProgram(UIReader reader)
        {
            var version = reader.Version;

            if (reader.IsLoveAndDeepspace())
            {
                var m_CodeHash = new Hash128(reader);
            }

            m_BlobIndex = reader.Reader.ReadUInt32();
            if (HasIsAdditionalBlob(reader.SerializedType))
            {
                var m_IsAdditionalBlob = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();
            }
            m_Channels = new ParserBindChannels(reader);

            if (version.GreaterThanOrEquals(2019) && version.LessThan(2021, 1) || HasGlobalLocalKeywordIndices(reader.SerializedType)) //2019 ~2021.1
            {
                var m_GlobalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.Reader.AlignStream();
                var m_LocalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.Reader.AlignStream();
            }
            else
            {
                m_KeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                if (version.GreaterThanOrEquals(2017)) //2017 and up
                {
                    reader.Reader.AlignStream();
                }
            }

            m_ShaderHardwareTier = reader.Reader.ReadSByte();
            m_GpuProgramType = (ShaderGpuProgramType)reader.Reader.ReadSByte();
            reader.Reader.AlignStream();

            if (reader.IsGI() && (m_GpuProgramType == ShaderGpuProgramType.Unknown || !Enum.IsDefined(typeof(ShaderGpuProgramType), m_GpuProgramType)))
            {
                reader.Position -= 4;
                var m_LocalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.Reader.AlignStream();

                m_ShaderHardwareTier = reader.Reader.ReadSByte();
                m_GpuProgramType = (ShaderGpuProgramType)reader.Reader.ReadSByte();
                reader.Reader.AlignStream();
            }

            if (version.GreaterThanOrEquals(2020, 3, 2, UnityVersionType.Final, 1) || //2020.3.2f1 and up
              version.GreaterThanOrEquals(2021, 1, 4, UnityVersionType.Final, 1)) //2021.1.4f1 and up
            {
                m_Parameters = new SerializedProgramParameters(reader);
            }
            else
            {
                int numVectorParams = reader.Reader.ReadInt32();
                m_VectorParams = new List<VectorParameter>();
                for (int i = 0; i < numVectorParams; i++)
                {
                    m_VectorParams.Add(new VectorParameter(reader));
                }

                int numMatrixParams = reader.Reader.ReadInt32();
                m_MatrixParams = new List<MatrixParameter>();
                for (int i = 0; i < numMatrixParams; i++)
                {
                    m_MatrixParams.Add(new MatrixParameter(reader));
                }

                int numTextureParams = reader.Reader.ReadInt32();
                m_TextureParams = new List<TextureParameter>();
                for (int i = 0; i < numTextureParams; i++)
                {
                    m_TextureParams.Add(new TextureParameter(reader));
                }

                int numBufferParams = reader.Reader.ReadInt32();
                m_BufferParams = new List<BufferBinding>();
                for (int i = 0; i < numBufferParams; i++)
                {
                    m_BufferParams.Add(new BufferBinding(reader));
                }

                int numConstantBuffers = reader.Reader.ReadInt32();
                m_ConstantBuffers = new List<ConstantBuffer>();
                for (int i = 0; i < numConstantBuffers; i++)
                {
                    m_ConstantBuffers.Add(new ConstantBuffer(reader));
                }

                int numConstantBufferBindings = reader.Reader.ReadInt32();
                m_ConstantBufferBindings = new List<BufferBinding>();
                for (int i = 0; i < numConstantBufferBindings; i++)
                {
                    m_ConstantBufferBindings.Add(new BufferBinding(reader));
                }

                int numUAVParams = reader.Reader.ReadInt32();
                m_UAVParams = new List<UAVParameter>();
                for (int i = 0; i < numUAVParams; i++)
                {
                    m_UAVParams.Add(new UAVParameter(reader.Reader));
                }

                if (version.GreaterThanOrEquals(2017)) //2017 and up
                {
                    int numSamplers = reader.Reader.ReadInt32();
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
                    var m_ShaderRequirements = reader.Reader.ReadInt64();
                }
                else
                {
                    var m_ShaderRequirements = reader.Reader.ReadInt32();
                }
            }

            if (HasInstancedStructuredBuffers(reader.SerializedType))
            {
                int numInstancedStructuredBuffers = reader.Reader.ReadInt32();
                var m_InstancedStructuredBuffers = new List<ConstantBuffer>();
                for (int i = 0; i < numInstancedStructuredBuffers; i++)
                {
                    m_InstancedStructuredBuffers.Add(new ConstantBuffer(reader));
                }
            }
        }
    }

    public class SerializedPlayerSubProgram
    {
        public uint m_BlobIndex;
        public ushort[] m_KeywordIndices;
        public long m_ShaderRequirements;
        public ShaderGpuProgramType m_GpuProgramType;

        public SerializedPlayerSubProgram(UIReader reader)
        {
            m_BlobIndex = reader.Reader.ReadUInt32();

            m_KeywordIndices = reader.ReadArray(r => r.ReadUInt16());
            reader.Reader.AlignStream();

            m_ShaderRequirements = reader.Reader.ReadInt64();
            m_GpuProgramType = (ShaderGpuProgramType)reader.Reader.ReadSByte();
            reader.Reader.AlignStream();
        }
    }

    public class SerializedProgram
    {
        public List<SerializedSubProgram> m_SubPrograms;
        public List<List<SerializedPlayerSubProgram>> m_PlayerSubPrograms;
        public uint[][] m_ParameterBlobIndices;
        public SerializedProgramParameters m_CommonParameters;
        public ushort[] m_SerializedKeywordStateMask;

        public SerializedProgram(UIReader reader)
        {
            var version = reader.Version;

            int numSubPrograms = reader.Reader.ReadInt32();
            m_SubPrograms = new List<SerializedSubProgram>();
            for (int i = 0; i < numSubPrograms; i++)
            {
                m_SubPrograms.Add(new SerializedSubProgram(reader));
            }

            if (version.GreaterThanOrEquals(2021, 3, 10, UnityVersionType.Final, 1) || //2021.3.10f1 and up
               version.GreaterThanOrEquals(2022, 1, 13, UnityVersionType.Final, 1)) //2022.1.13f1 and up
            {
                int numPlayerSubPrograms = reader.Reader.ReadInt32();
                m_PlayerSubPrograms = new List<List<SerializedPlayerSubProgram>>();
                for (int i = 0; i < numPlayerSubPrograms; i++)
                {
                    m_PlayerSubPrograms.Add(new List<SerializedPlayerSubProgram>());
                    int numPlatformPrograms = reader.Reader.ReadInt32();
                    for (int j = 0; j < numPlatformPrograms; j++)
                    {
                        m_PlayerSubPrograms[i].Add(new SerializedPlayerSubProgram(reader));
                    }
                }

                m_ParameterBlobIndices = reader.Reader.ReadArrayArray(r => r.ReadUInt32());
            }

            if (version.GreaterThanOrEquals(2020, 3, 2, UnityVersionType.Final, 1) || //2020.3.2f1 and up
               version.GreaterThanOrEquals(2021, 1, 1, UnityVersionType.Final, 1)) //2021.1.1f1 and up
            {
                m_CommonParameters = new SerializedProgramParameters(reader);
            }

            if (version.GreaterThanOrEquals(2022, 1)) //2022.1 and up
            {
                m_SerializedKeywordStateMask = reader.ReadArray(r => r.ReadUInt16());
                reader.Reader.AlignStream();
            }
        }
    }

    public enum PassType
    {
        Normal = 0,
        Use = 1,
        Grab = 2
    };

    public class SerializedPass
    {
        public List<Hash128> m_EditorDataHash;
        public byte[] m_Platforms;
        public ushort[] m_LocalKeywordMask;
        public ushort[] m_GlobalKeywordMask;
        public List<KeyValuePair<string, int>> m_NameIndices;
        public PassType m_Type;
        public SerializedShaderState m_State;
        public uint m_ProgramMask;
        public SerializedProgram progVertex;
        public SerializedProgram progFragment;
        public SerializedProgram progGeometry;
        public SerializedProgram progHull;
        public SerializedProgram progDomain;
        public SerializedProgram progRayTracing;
        public bool m_HasInstancingVariant;
        public string m_UseName;
        public string m_Name;
        public string m_TextureName;
        public SerializedTagMap m_Tags;
        public ushort[] m_SerializedKeywordStateMask;

        public SerializedPass(UIReader reader)
        {
            var version = reader.Version;

            if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                int numEditorDataHash = reader.Reader.ReadInt32();
                m_EditorDataHash = new List<Hash128>();
                for (int i = 0; i < numEditorDataHash; i++)
                {
                    m_EditorDataHash.Add(new Hash128(reader));
                }
                reader.Reader.AlignStream();
                m_Platforms = reader.ReadArray(r => r.ReadByte());
                reader.Reader.AlignStream();
                if (version.LessThan(2021, 1)) //2021.1 and down
                {
                    m_LocalKeywordMask = reader.ReadArray(r => r.ReadUInt16());
                    reader.Reader.AlignStream();
                    m_GlobalKeywordMask = reader.ReadArray(r => r.ReadUInt16());
                    reader.Reader.AlignStream();
                }
            }

            int numIndices = reader.Reader.ReadInt32();
            m_NameIndices = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < numIndices; i++)
            {
                m_NameIndices.Add(new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.Reader.ReadInt32()));
            }

            m_Type = (PassType)reader.Reader.ReadInt32();
            m_State = new SerializedShaderState(reader);
            m_ProgramMask = reader.Reader.ReadUInt32();
            progVertex = new SerializedProgram(reader);
            progFragment = new SerializedProgram(reader);
            progGeometry = new SerializedProgram(reader);
            progHull = new SerializedProgram(reader);
            progDomain = new SerializedProgram(reader);
            if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
            {
                progRayTracing = new SerializedProgram(reader);
            }
            m_HasInstancingVariant = reader.Reader.ReadBoolean();
            if (version.GreaterThanOrEquals(2018)) //2018 and up
            {
                var m_HasProceduralInstancingVariant = reader.Reader.ReadBoolean();
            }
            reader.Reader.AlignStream();
            m_UseName = reader.ReadAlignedString();
            m_Name = reader.ReadAlignedString();
            m_TextureName = reader.ReadAlignedString();
            m_Tags = new SerializedTagMap(reader);
            if (version.Major == 2021 && version.Minor >= 2) //2021.2 ~2021.x
            {
                m_SerializedKeywordStateMask = reader.ReadArray(r => r.ReadUInt16());
                reader.Reader.AlignStream();
            }
        }
    }

    public class SerializedTagMap
    {
        public List<KeyValuePair<string, string>> tags;

        public SerializedTagMap(UIReader reader)
        {
            int numTags = reader.Reader.ReadInt32();
            tags = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < numTags; i++)
            {
                tags.Add(new KeyValuePair<string, string>(reader.ReadAlignedString(), reader.ReadAlignedString()));
            }
        }
    }

    public class SerializedSubShader
    {
        public List<SerializedPass> m_Passes;
        public SerializedTagMap m_Tags;
        public int m_LOD;

        public SerializedSubShader(UIReader reader)
        {
            int numPasses = reader.Reader.ReadInt32();
            m_Passes = new List<SerializedPass>();
            for (int i = 0; i < numPasses; i++)
            {
                m_Passes.Add(new SerializedPass(reader));
            }

            m_Tags = new SerializedTagMap(reader);
            m_LOD = reader.Reader.ReadInt32();
        }
    }

    public class SerializedShaderDependency
    {
        public string from;
        public string to;

        public SerializedShaderDependency(UIReader reader)
        {
            from = reader.ReadAlignedString();
            to = reader.ReadAlignedString();
        }
    }

    public class SerializedCustomEditorForRenderPipeline
    {
        public string customEditorName;
        public string renderPipelineType;

        public SerializedCustomEditorForRenderPipeline(UIReader reader)
        {
            customEditorName = reader.ReadAlignedString();
            renderPipelineType = reader.ReadAlignedString();
        }
    }

    public class SerializedShader
    {
        public SerializedProperties m_PropInfo;
        public List<SerializedSubShader> m_SubShaders;
        public string[] m_KeywordNames;
        public byte[] m_KeywordFlags;
        public string m_Name;
        public string m_CustomEditorName;
        public string m_FallbackName;
        public List<SerializedShaderDependency> m_Dependencies;
        public List<SerializedCustomEditorForRenderPipeline> m_CustomEditorForRenderPipelines;
        public bool m_DisableNoSubshadersMessage;

        public SerializedShader(UIReader reader)
        {
            var version = reader.Version;

            m_PropInfo = new SerializedProperties(reader);

            int numSubShaders = reader.Reader.ReadInt32();
            m_SubShaders = new List<SerializedSubShader>();
            for (int i = 0; i < numSubShaders; i++)
            {
                m_SubShaders.Add(new SerializedSubShader(reader));
            }

            if (version.GreaterThanOrEquals(2021, 2)) //2021.2 and up
            {
                m_KeywordNames = reader.ReadArray(r => r.ReadString());
                m_KeywordFlags = reader.ReadArray(r => r.ReadByte());
                reader.Reader.AlignStream();
            }

            m_Name = reader.ReadAlignedString();
            m_CustomEditorName = reader.ReadAlignedString();
            m_FallbackName = reader.ReadAlignedString();

            int numDependencies = reader.Reader.ReadInt32();
            m_Dependencies = new List<SerializedShaderDependency>();
            for (int i = 0; i < numDependencies; i++)
            {
                m_Dependencies.Add(new SerializedShaderDependency(reader));
            }

            if (version.GreaterThanOrEquals(2021, 1)) //2021.1 and up
            {
                int m_CustomEditorForRenderPipelinesSize = reader.Reader.ReadInt32();
                m_CustomEditorForRenderPipelines = new List<SerializedCustomEditorForRenderPipeline>();
                for (int i = 0; i < m_CustomEditorForRenderPipelinesSize; i++)
                {
                    m_CustomEditorForRenderPipelines.Add(new SerializedCustomEditorForRenderPipeline(reader));
                }
            }

            m_DisableNoSubshadersMessage = reader.Reader.ReadBoolean();
            reader.Reader.AlignStream();
        }
    }

    public enum ShaderCompilerPlatform
    {
        None = -1,
        GL = 0,
        D3D9 = 1,
        Xbox360 = 2,
        PS3 = 3,
        D3D11 = 4,
        GLES20 = 5,
        NaCl = 6,
        Flash = 7,
        D3D11_9x = 8,
        GLES3Plus = 9,
        PSP2 = 10,
        PS4 = 11,
        XboxOne = 12,
        PSM = 13,
        Metal = 14,
        OpenGLCore = 15,
        N3DS = 16,
        WiiU = 17,
        Vulkan = 18,
        Switch = 19,
        XboxOneD3D12 = 20,
        GameCoreXboxOne = 21,
        GameCoreScarlett = 22,
        PS5 = 23,
        PS5NGGC = 24
    };

    public class Shader : NamedObject, IFileWriter
    {
        public byte[] m_Script;
        //5.3 - 5.4
        public uint decompressedSize;
        public byte[] m_SubProgramBlob;
        //5.5 and up
        public SerializedShader m_ParsedForm;
        public ShaderCompilerPlatform[] platforms;
        public uint[][] offsets;
        public uint[][] compressedLengths;
        public uint[][] decompressedLengths;
        public byte[] compressedBlob;
        public uint[] stageCounts;

        public override string Name => m_ParsedForm?.m_Name ?? m_Name;

        public Shader(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            if (version.GreaterThanOrEquals(5, 5)) //5.5 and up
            {
                m_ParsedForm = new SerializedShader(reader);
                platforms = reader.ReadArray(r => r.ReadUInt32()).Select(x => (ShaderCompilerPlatform)x).ToArray();
                if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
                {
                    offsets = reader.Reader.ReadArrayArray(r => r.ReadUInt32());
                    compressedLengths = reader.Reader.ReadArrayArray(r => r.ReadUInt32());
                    decompressedLengths = reader.Reader.ReadArrayArray(r => r.ReadUInt32());
                }
                else
                {
                    offsets = reader.ReadArray(r => r.ReadUInt32()).Select(x => new[] { x }).ToArray();
                    compressedLengths = reader.ReadArray(r => r.ReadUInt32()).Select(x => new[] { x }).ToArray();
                    decompressedLengths = reader.ReadArray(r => r.ReadUInt32()).Select(x => new[] { x }).ToArray();
                }
                compressedBlob = reader.ReadArray(r => r.ReadByte());
                reader.Reader.AlignStream();
                if (reader.IsGISubGroup())
                {
                    if (BinaryPrimitives.ReadInt32LittleEndian(compressedBlob) == -1)
                    {
                        compressedBlob = reader.ReadArray(r => r.ReadByte()); //blobDataBlocks
                        reader.Reader.AlignStream();
                    }
                }

                if (reader.IsLoveAndDeepspace())
                {
                    var codeOffsets = reader.Reader.ReadArrayArray(r => r.ReadUInt32());
                    var codeCompressedLengths = reader.Reader.ReadArrayArray(r => r.ReadUInt32());
                    var codeDecompressedLengths = reader.Reader.ReadArrayArray(r => r.ReadUInt32());
                    var codeCompressedBlob = reader.ReadArray(r => r.ReadByte());
                    reader.Reader.AlignStream();
                }

                if (version.GreaterThanOrEquals(2021, 3, 12, UnityVersionType.Final, 1) || //2021.3.12f1 and up
                    version.GreaterThanOrEquals(2022, 1, 21, UnityVersionType.Final, 1)) //2022.1.21f1 and up
                {
                    stageCounts = reader.ReadArray(r => r.ReadUInt32());
                }

                var m_DependenciesCount = reader.Reader.ReadInt32();
                for (int i = 0; i < m_DependenciesCount; i++)
                {
                    new PPtr<Shader>(reader);
                }

                if (version.GreaterThanOrEquals(2018))
                {
                    var m_NonModifiableTexturesCount = reader.Reader.ReadInt32();
                    for (int i = 0; i < m_NonModifiableTexturesCount; i++)
                    {
                        var first = reader.ReadAlignedString();
                        new PPtr<Texture>(reader);
                    }
                }

                var m_ShaderIsBaked = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();
            }
            else
            {
                m_Script = reader.ReadArray(r => r.ReadByte());
                reader.Reader.AlignStream();
                var m_PathName = reader.ReadAlignedString();
                if (version.Major == 5 && version.Minor >= 3) //5.3 - 5.4
                {
                    decompressedSize = reader.Reader.ReadUInt32();
                    m_SubProgramBlob = reader.ReadArray(r => r.ReadByte());
                }
            }
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".shader", mode, out fileName))
            {
                return;
            }
            //File.WriteAllText(fileName, );
        }
    }
}
