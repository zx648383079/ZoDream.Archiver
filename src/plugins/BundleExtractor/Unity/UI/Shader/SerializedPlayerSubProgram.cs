﻿using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedPlayerSubProgram
    {
        public uint m_BlobIndex;
        public ushort[] m_KeywordIndices;
        public long m_ShaderRequirements;
        public ShaderGpuProgramType m_GpuProgramType;

        public SerializedPlayerSubProgram(IBundleBinaryReader reader)
        {
            m_BlobIndex = reader.ReadUInt32();

            m_KeywordIndices = reader.ReadArray(r => r.ReadUInt16());
            reader.AlignStream();

            m_ShaderRequirements = reader.ReadInt64();
            m_GpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
            reader.AlignStream();
        }
    }

}
