using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Language;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class ShaderSubProgram
    {
        private readonly Version _version;
        public ShaderGpuProgramType m_ProgramType;
        public string[] m_Keywords;
        public string[] m_LocalKeywords;
        public byte[] m_ProgramCode;

        private readonly bool _hasError;
        public ShaderSubProgram(IBundleBinaryReader reader)
        {
            _version = reader.Get<Version>();
            var versionNo = reader.ReadInt32();
            Debug.Assert(GetExpectedProgramVersion(_version) == versionNo);
            m_ProgramType = (ShaderGpuProgramType)reader.ReadInt32();
            var statsALU = reader.ReadInt32();
            var statsTEX = reader.ReadInt32();
            var statsFlow = reader.ReadInt32();
            if (_version.GreaterThanOrEquals(5, 5))
            {
                var statsTempRegister = reader.ReadInt32();
            }
            var m_KeywordsSize = reader.ReadInt32();
            if (m_KeywordsSize > 500)
            {
                _hasError = true;
                // TODO 遇到不合规的
                return;
            }
            var pos = reader.Position;
            reader.Position++;
            if (reader.ReadByte() != 0)
            {
                _hasError = true;
                // TODO 遇到不合规的
                return;
            }
            reader.Position = pos;
            try
            {
                m_Keywords = new string[m_KeywordsSize];
                for (int i = 0; i < m_KeywordsSize; i++)
                {
                    m_Keywords[i] = reader.ReadAlignedString();
                }
            }
            catch (EndOfStreamException)
            {
                m_Keywords = [];
                _hasError = true;
                // TODO 遇到不合规的
                return;
            }
            if (_version.LessThan(2021, 2) && _version.GreaterThanOrEquals(2019))
            {
                var m_LocalKeywordsSize = reader.ReadInt32();
                m_LocalKeywords = new string[m_LocalKeywordsSize];
                for (int i = 0; i < m_LocalKeywordsSize; i++)
                {
                    m_LocalKeywords[i] = reader.ReadAlignedString();
                }
            }
            m_ProgramCode = reader.ReadArray(r => r.ReadByte());
            reader.AlignStream();

            var sourceMap = reader.ReadInt32();
            var bindCount = reader.ReadInt32();
            for (int i = 0; i < bindCount; i++)
            {
                var source = reader.ReadUInt32();
                var target = reader.ReadUInt32();
                sourceMap |= 1 << (int)source;
            }
        }

        public void Write(ICodeWriter writer)
        {
            if (_hasError)
            {
                writer.Write("// has some error, don't disassemble").WriteLine(true);
                return;
            }
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
                            SpirVShader.Convert(m_ProgramCode, _version, writer);
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

        private static int GetExpectedProgramVersion(Version version)
        {
            return version switch
            {
                _ when version.Equals(5, 3) => 201509030,
                _ when version.Equals(5, 4) => 201510240,
                _ when version.Equals(5, 5) => 201608170,
                _ when version.LessThan(2017, 3) => 201609010,
                _ when version.LessThan(2018, 2) => 201708220,
                _ when version.LessThan(2019) => 201802150,
                _ when version.LessThan(2021, 2) => 201806140,
                _ when version.LessThan(2022, 3) => 202012090,
                _ => 202310270,
            };
        }
    }
}