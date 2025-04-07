using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class ShaderSubProgramWrap
    {
        private byte[] buffer;
        private ShaderSubProgramEntry entry;

        public ShaderSubProgramWrap(IBundleBinaryReader reader, ShaderSubProgramEntry paramEntry)
        {
            entry = paramEntry;
            buffer = new byte[entry.Length];
            reader.BaseStream.ReadExactly(buffer, 0, entry.Length);
        }

        public ShaderSubProgram GenShaderSubProgram()
        {
            ShaderSubProgram shaderSubProgram = null;
            using (var reader = new BundleBinaryReader(new MemoryStream(buffer), leaveOpen: false))
            {
                shaderSubProgram = new ShaderSubProgram(reader);
            }
            return shaderSubProgram;
        }
    }
}