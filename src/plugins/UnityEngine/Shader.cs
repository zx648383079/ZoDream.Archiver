
namespace UnityEngine
{
    public class Shader : Object
    {
        public byte[] Script;
        //5.3 - 5.4
        public uint DecompressedSize;
        public byte[] SubProgramBlob;
        //5.5 and up
        public SerializedShader ParsedForm;
        public ShaderCompilerPlatform[] Platforms;
        public uint[][] Offsets;
        public uint[][] CompressedLengths;
        public uint[][] DecompressedLengths;
        public byte[] CompressedBlob;
        public uint[] StageCounts;
    }
}
