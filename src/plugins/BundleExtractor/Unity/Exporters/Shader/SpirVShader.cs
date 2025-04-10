using System.IO;
using ZoDream.ShaderDecompiler;
using ZoDream.Shared.IO;
using ZoDream.Shared.Language;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal static class SpirVShader
    {
        public static void Convert(byte[] m_ProgramCode, ICodeWriter builder)
        {
            using (var ms = new MemoryStream(m_ProgramCode))
            {
                using var reader = new BinaryReader(ms);
                int requirements = reader.ReadInt32();
                int minOffset = m_ProgramCode.Length;
                int snippetCount = 5;
                /*if (version[0] > 2019 || (version[0] == 2019 && version[1] >= 3)) //2019.3 and up
                {
                    snippetCount = 6;
                }*/
                for (int i = 0; i < snippetCount; i++)
                {
                    if (reader.BaseStream.Position >= minOffset)
                    {
                        break;
                    }

                    int offset = reader.ReadInt32();
                    int size = reader.ReadInt32();
                    if (size > 0)
                    {
                        if (offset < minOffset)
                        {
                            minOffset = offset;
                        }
                        var pos = ms.Position;
                        ShaderScheme.Disassemble(new PartialStream(ms, offset, size), builder);
                        ms.Position = pos;
                    }
                }
            }
        }

    }
}