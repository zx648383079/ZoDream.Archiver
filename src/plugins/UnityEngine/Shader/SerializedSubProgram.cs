
namespace UnityEngine
{
    public class SerializedSubProgram
    {
        public uint BlobIndex;
        public ParserBindChannels Channels;
        public ushort[] KeywordIndices;
        public sbyte ShaderHardwareTier;
        public ShaderGpuProgramType GpuProgramType;
        public SerializedProgramParameters Parameters;
        public VectorParameter[] VectorParams;
        public MatrixParameter[] MatrixParams;
        public TextureParameter[] TextureParams;
        public BufferBinding[] BufferParams;
        public ConstantBuffer[] ConstantBuffers;
        public BufferBinding[] ConstantBufferBindings;
        public UAVParameter[] UAVParams;
        public SamplerParameter[] Samplers;

    }

}
