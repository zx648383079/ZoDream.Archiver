using System;
using System.Linq;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Shader(UIReader reader) : NamedObject(reader), IElementLoader, IFileExporter
    {
        public byte[] m_Script;
        //5.3 - 5.4
        public uint decompressedSize;
        public byte[] m_SubProgramBlob;
        //5.5 and up
        public SerializedShader m_ParsedForm;
        public ShaderCompilerPlatform[] platforms;
        public uint[][] offsets;
        public uint[][] compressedLengths;
        public uint[][] decompressedLengths;
        public byte[] compressedBlob;
        public uint[] stageCounts;

        public override string Name => m_ParsedForm?.m_Name ?? m_Name;

        public void ReadBase(IBundleBinaryReader reader, Action cb)
        {
            base.Read(reader);
            var version = reader.Get<UnityVersion>();
            if (version.GreaterThanOrEquals(5, 5)) //5.5 and up
            {
                m_ParsedForm = new SerializedShader(reader);
                platforms = reader.ReadArray(r => r.ReadUInt32()).Select(x => (ShaderCompilerPlatform)x).ToArray();
                if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
                {
                    offsets = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                    compressedLengths = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                    decompressedLengths = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                }
                else
                {
                    offsets = reader.ReadArray(r => r.ReadUInt32()).Select(x => new[] { x }).ToArray();
                    compressedLengths = reader.ReadArray(r => r.ReadUInt32()).Select(x => new[] { x }).ToArray();
                    decompressedLengths = reader.ReadArray(r => r.ReadUInt32()).Select(x => new[] { x }).ToArray();
                }
                compressedBlob = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
                cb.Invoke();

                if (version.GreaterThanOrEquals(2021, 3, 12, UnityVersionType.Final, 1) || //2021.3.12f1 and up
                    version.GreaterThanOrEquals(2022, 1, 21, UnityVersionType.Final, 1)) //2022.1.21f1 and up
                {
                    stageCounts = reader.ReadArray(r => r.ReadUInt32());
                }

                var m_DependenciesCount = reader.ReadInt32();
                for (int i = 0; i < m_DependenciesCount; i++)
                {
                    _ = new PPtr<Shader>(reader);
                }

                if (version.GreaterThanOrEquals(2018))
                {
                    var m_NonModifiableTexturesCount = reader.ReadInt32();
                    for (int i = 0; i < m_NonModifiableTexturesCount; i++)
                    {
                        var first = reader.ReadAlignedString();
                        new PPtr<Texture>(reader);
                    }
                }

                var m_ShaderIsBaked = reader.ReadBoolean();
                reader.AlignStream();
            }
            else
            {
                m_Script = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
                var m_PathName = reader.ReadAlignedString();
                if (version.Major == 5 && version.Minor >= 3) //5.3 - 5.4
                {
                    decompressedSize = reader.ReadUInt32();
                    m_SubProgramBlob = reader.ReadArray(r => r.ReadByte());
                }
            }
        }

        public override void Read(IBundleBinaryReader reader)
        {
            ReadBase(reader, () => {
            });
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".shader", mode, out fileName))
            {
                return;
            }
            //File.WriteAllText(fileName, );
            // TODO
        }
    }
}
