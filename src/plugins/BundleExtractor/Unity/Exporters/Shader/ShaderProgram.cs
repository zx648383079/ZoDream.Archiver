using System.Text.RegularExpressions;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Language;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal partial class ShaderProgram
    {
        public ShaderSubProgramEntry[] entries;
        public ShaderSubProgramWrap[] m_SubProgramWraps;

        public ShaderProgram(IBundleBinaryReader reader)
        {
            var subProgramsCapacity = reader.ReadInt32();
            entries = new ShaderSubProgramEntry[subProgramsCapacity];
            for (int i = 0; i < subProgramsCapacity; i++)
            {
                entries[i] = new ShaderSubProgramEntry(reader);
            }
            m_SubProgramWraps = new ShaderSubProgramWrap[subProgramsCapacity];
        }

        public void Read(IBundleBinaryReader reader, int segment)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry.Segment == segment)
                {
                    m_SubProgramWraps[i] = new ShaderSubProgramWrap(reader, entry);
                }
            }
        }

        public void Write(string shader, ICodeWriter writer)
        {
            var match = GPUIndexRegex().Match(shader);
            var begin = 0;
            while (match is not null && match.Success)
            {
                writer.Write(shader[begin..match.Index]);
                var index = int.Parse(match.Groups[1].Value);
                var subProgramWrap = m_SubProgramWraps[index];
                var subProgram = subProgramWrap.GenShaderSubProgram();
                subProgram.Write(writer);
                begin = match.Index + match.Value.Length;
                match = match.NextMatch();
            }
            if (begin < shader.Length)
            {
                writer.Write(shader[begin..]);
            }
        }

        [GeneratedRegex("GpuProgramIndex (.+)")]
        private static partial Regex GPUIndexRegex();
    }
}
