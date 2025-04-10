using System;
using System.IO;
using System.Text;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Language;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class ShaderSubProgram
    {
        private int m_Version;
        public ShaderGpuProgramType m_ProgramType;
        public string[] m_Keywords;
        public string[] m_LocalKeywords;
        public byte[] m_ProgramCode;

        private static int i = 0;
        public ShaderSubProgram(IBundleBinaryReader reader)
        {
            //LoadGpuProgramFromData
            //201509030 - Unity 5.3
            //201510240 - Unity 5.4
            //201608170 - Unity 5.5
            //201609010 - Unity 5.6, 2017.1 & 2017.2
            //201708220 - Unity 2017.3, Unity 2017.4 & Unity 2018.1
            //201802150 - Unity 2018.2 & Unity 2018.3
            //201806140 - Unity 2019.1~2021.1
            //202012090 - Unity 2021.2
            m_Version = reader.ReadInt32();
            m_ProgramType = (ShaderGpuProgramType)reader.ReadInt32();
            reader.BaseStream.Position += 12;
            if (m_Version >= 201608170)
            {
                reader.BaseStream.Position += 4;
            }
            var m_KeywordsSize = reader.ReadInt32();
            m_Keywords = new string[m_KeywordsSize];
            for (int i = 0; i < m_KeywordsSize; i++)
            {
                m_Keywords[i] = reader.ReadAlignedString();
            }
            if (m_Version >= 201806140 && m_Version < 202012090)
            {
                var m_LocalKeywordsSize = reader.ReadInt32();
                m_LocalKeywords = new string[m_LocalKeywordsSize];
                for (int i = 0; i < m_LocalKeywordsSize; i++)
                {
                    m_LocalKeywords[i] = reader.ReadAlignedString();
                }
            }
            m_ProgramCode = reader.ReadArray((r, _) => r.ReadByte());
            reader.AlignStream();

            //TODO
        }

        public void Write(ICodeWriter writer)
        {
            if (m_Keywords.Length > 0)
            {
                writer.Write("Keywords { ");
                foreach (string keyword in m_Keywords)
                {
                    writer.Write($"\"{keyword}\" ");
                }
                writer.Write("}")
                    .WriteLine(true);
            }
            if (m_LocalKeywords != null && m_LocalKeywords.Length > 0)
            {
                writer.Write("Local Keywords { ");
                foreach (string keyword in m_LocalKeywords)
                {
                    writer.Write($"\"{keyword}\" ");
                }
                writer.Write("}").WriteLine(true);
            }

            writer.Write('"');
            if (m_ProgramCode.Length > 0)
            {
                switch (m_ProgramType)
                {
                    case ShaderGpuProgramType.GLLegacy:
                    case ShaderGpuProgramType.GLES31AEP:
                    case ShaderGpuProgramType.GLES31:
                    case ShaderGpuProgramType.GLES3:
                    case ShaderGpuProgramType.GLES:
                    case ShaderGpuProgramType.GLCore32:
                    case ShaderGpuProgramType.GLCore41:
                    case ShaderGpuProgramType.GLCore43:
                        writer.Write(m_ProgramCode);
                        break;
                    case ShaderGpuProgramType.DX9VertexSM20:
                    case ShaderGpuProgramType.DX9VertexSM30:
                    case ShaderGpuProgramType.DX9PixelSM20:
                    case ShaderGpuProgramType.DX9PixelSM30:
                        {
                            /*var shaderBytecode = new ShaderBytecode(m_ProgramCode);
                            sb.Append(shaderBytecode.Disassemble());*/
                            writer.Write("// shader disassembly not supported on DXBC");
                            break;
                        }
                    case ShaderGpuProgramType.DX10Level9Vertex:
                    case ShaderGpuProgramType.DX10Level9Pixel:
                    case ShaderGpuProgramType.DX11VertexSM40:
                    case ShaderGpuProgramType.DX11VertexSM50:
                    case ShaderGpuProgramType.DX11PixelSM40:
                    case ShaderGpuProgramType.DX11PixelSM50:
                    case ShaderGpuProgramType.DX11GeometrySM40:
                    case ShaderGpuProgramType.DX11GeometrySM50:
                    case ShaderGpuProgramType.DX11HullSM50:
                    case ShaderGpuProgramType.DX11DomainSM50:
                        {
                            /*int start = 6;
                            if (m_Version == 201509030) // 5.3
                            {
                                start = 5;
                            }
                            var buff = new byte[m_ProgramCode.Length - start];
                            Buffer.BlockCopy(m_ProgramCode, start, buff, 0, buff.Length);
                            var shaderBytecode = new ShaderBytecode(buff);
                            sb.Append(shaderBytecode.Disassemble());*/
                            writer.Write("// shader disassembly not supported on DXBC");
                            break;
                        }
                    case ShaderGpuProgramType.MetalVS:
                    case ShaderGpuProgramType.MetalFS:
                        using (var reader = new BundleBinaryReader(new MemoryStream(m_ProgramCode), leaveOpen: false))
                        {
                            var fourCC = reader.ReadUInt32();
                            if (fourCC == 0xf00dcafe)
                            {
                                int offset = reader.ReadInt32();
                                reader.BaseStream.Position = offset;
                            }
                            var entryName = reader.ReadStringZeroTerm();
                            var buff = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                            writer.Write(buff);
                        }
                        break;
                    case ShaderGpuProgramType.SPIRV:
                        try
                        {
                            SpirVShader.Convert(m_ProgramCode, writer);
                        }
                        catch (Exception e)
                        {
                            writer.Write($"// disassembly error {e.Message}").WriteLine(true);
                        }
                        break;
                    case ShaderGpuProgramType.ConsoleVS:
                    case ShaderGpuProgramType.ConsoleFS:
                    case ShaderGpuProgramType.ConsoleHS:
                    case ShaderGpuProgramType.ConsoleDS:
                    case ShaderGpuProgramType.ConsoleGS:
                        writer.Write(m_ProgramCode);
                        break;
                    default:
                        writer.Write($"//shader disassembly not supported on {m_ProgramType}");
                        break;
                }
            }
            writer.Write('"').WriteLine(true);
        }
    }
}