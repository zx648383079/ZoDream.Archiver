using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class ShaderSubProgramWrap
    {
        private readonly ShaderSubProgram subProgram;
        private readonly ShaderSubProgramEntry entry;

        public ShaderSubProgramWrap(IBundleBinaryReader reader, ShaderSubProgramEntry paramEntry)
        {
            entry = paramEntry;
            var pos = reader.Position;
            subProgram = new ShaderSubProgram(new BundleBinaryReader(
                new PartialStream(reader.BaseStream, pos, entry.Length),
                reader
                ));
            reader.Position = pos + entry.Length;
        }

        public ShaderSubProgram GenShaderSubProgram()
        {
            return subProgram;
        }
    }
}