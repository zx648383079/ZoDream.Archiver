using System.Text.RegularExpressions;
using ZoDream.Shared.Bundle;

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

        public string Export(string shader)
        {
            var evaluator = new MatchEvaluator(match => {
                var index = int.Parse(match.Groups[1].Value);
                var subProgramWrap = m_SubProgramWraps[index];
                var subProgram = subProgramWrap.GenShaderSubProgram();
                var subProgramsStr = subProgram.Export();
                return subProgramsStr;
            });
            shader = GPUIndexRegex().Replace(shader, evaluator);
            return shader;
        }

        [GeneratedRegex("GpuProgramIndex (.+)")]
        private static partial Regex GPUIndexRegex();
    }
}
