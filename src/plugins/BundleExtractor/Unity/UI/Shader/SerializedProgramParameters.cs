using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedProgramParameters
    {
        public List<VectorParameter> m_VectorParams;
        public List<MatrixParameter> m_MatrixParams;
        public List<TextureParameter> m_TextureParams;
        public List<BufferBinding> m_BufferParams;
        public List<ConstantBuffer> m_ConstantBuffers;
        public List<BufferBinding> m_ConstantBufferBindings;
        public List<UAVParameter> m_UAVParams;
        public List<SamplerParameter> m_Samplers;

        public SerializedProgramParameters(IBundleBinaryReader reader)
        {
            int numVectorParams = reader.ReadInt32();
            m_VectorParams = [];
            var scanner = reader.Get<IBundleElementScanner>();
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams.Add(scanner.CreateElement<VectorParameter>(reader));
            }

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = [];
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams.Add(scanner.CreateElement<MatrixParameter>(reader));
            }

            int numTextureParams = reader.ReadInt32();
            m_TextureParams = [];
            for (int i = 0; i < numTextureParams; i++)
            {
                m_TextureParams.Add(new TextureParameter(reader));
            }

            int numBufferParams = reader.ReadInt32();
            m_BufferParams = [];
            for (int i = 0; i < numBufferParams; i++)
            {
                m_BufferParams.Add(new BufferBinding(reader));
            }

            int numConstantBuffers = reader.ReadInt32();
            m_ConstantBuffers = [];
            for (int i = 0; i < numConstantBuffers; i++)
            {
                var instance = new ConstantBuffer();
                scanner.TryRead(reader, instance);
                m_ConstantBuffers.Add(instance);
            }

            int numConstantBufferBindings = reader.ReadInt32();
            m_ConstantBufferBindings = [];
            for (int i = 0; i < numConstantBufferBindings; i++)
            {
                m_ConstantBufferBindings.Add(new BufferBinding(reader));
            }

            int numUAVParams = reader.ReadInt32();
            m_UAVParams = [];
            for (int i = 0; i < numUAVParams; i++)
            {
                m_UAVParams.Add(new UAVParameter(reader));
            }

            int numSamplers = reader.ReadInt32();
            m_Samplers = [];
            for (int i = 0; i < numSamplers; i++)
            {
                m_Samplers.Add(new SamplerParameter(reader));
            }
        }
    }

}
