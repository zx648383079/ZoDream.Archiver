using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZoDream.BundleExtractor.SerializedFiles;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.UI
{
    public sealed class Font : NamedObject, IFileWriter
    {
        public byte[] m_FontData;

        public Font(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            if (version.GreaterThanOrEquals(5, 5))//5.5 and up
            {
                var m_LineSpacing = reader.Reader.ReadSingle();
                var m_DefaultMaterial = new PPtr<Material>(reader);
                var m_FontSize = reader.Reader.ReadSingle();
                var m_Texture = new PPtr<Texture>(reader);
                int m_AsciiStartOffset = reader.Reader.ReadInt32();
                var m_Tracking = reader.Reader.ReadSingle();
                var m_CharacterSpacing = reader.Reader.ReadInt32();
                var m_CharacterPadding = reader.Reader.ReadInt32();
                var m_ConvertCase = reader.Reader.ReadInt32();
                int m_CharacterRects_size = reader.Reader.ReadInt32();
                for (int i = 0; i < m_CharacterRects_size; i++)
                {
                    reader.Position += 44;//CharacterInfo data 41
                }
                int m_KerningValues_size = reader.Reader.ReadInt32();
                for (int i = 0; i < m_KerningValues_size; i++)
                {
                    reader.Position += 8;
                }
                var m_PixelScale = reader.Reader.ReadSingle();
                int m_FontData_size = reader.Reader.ReadInt32();
                if (m_FontData_size > 0)
                {
                    m_FontData = reader.Reader.ReadBytes(m_FontData_size);
                }
            }
            else
            {
                int m_AsciiStartOffset = reader.Reader.ReadInt32();

                if (version.LessThanOrEquals(3))
                {
                    int m_FontCountX = reader.Reader.ReadInt32();
                    int m_FontCountY = reader.Reader.ReadInt32();
                }

                float m_Kerning = reader.Reader.ReadSingle();
                float m_LineSpacing = reader.Reader.ReadSingle();

                if (version.LessThanOrEquals(3))
                {
                    int m_PerCharacterKerning_size = reader.Reader.ReadInt32();
                    for (int i = 0; i < m_PerCharacterKerning_size; i++)
                    {
                        int first = reader.Reader.ReadInt32();
                        float second = reader.Reader.ReadSingle();
                    }
                }
                else
                {
                    int m_CharacterSpacing = reader.Reader.ReadInt32();
                    int m_CharacterPadding = reader.Reader.ReadInt32();
                }

                int m_ConvertCase = reader.Reader.ReadInt32();
                var m_DefaultMaterial = new PPtr<Material>(reader);

                int m_CharacterRects_size = reader.Reader.ReadInt32();
                for (int i = 0; i < m_CharacterRects_size; i++)
                {
                    int index = reader.Reader.ReadInt32();
                    //Rectf uv
                    float uvx = reader.Reader.ReadSingle();
                    float uvy = reader.Reader.ReadSingle();
                    float uvwidth = reader.Reader.ReadSingle();
                    float uvheight = reader.Reader.ReadSingle();
                    //Rectf vert
                    float vertx = reader.Reader.ReadSingle();
                    float verty = reader.Reader.ReadSingle();
                    float vertwidth = reader.Reader.ReadSingle();
                    float vertheight = reader.Reader.ReadSingle();
                    float width = reader.Reader.ReadSingle();

                    if (version.GreaterThanOrEquals(4))
                    {
                        var flipped = reader.Reader.ReadBoolean();
                        reader.Reader.AlignStream();
                    }
                }

                var m_Texture = new PPtr<Texture>(reader);

                int m_KerningValues_size = reader.Reader.ReadInt32();
                for (int i = 0; i < m_KerningValues_size; i++)
                {
                    int pairfirst = reader.Reader.ReadInt16();
                    int pairsecond = reader.Reader.ReadInt16();
                    float second = reader.Reader.ReadSingle();
                }

                if (version.LessThanOrEquals(3))
                {
                    var m_GridFont = reader.Reader.ReadBoolean();
                    reader.Reader.AlignStream();
                }
                else { float m_PixelScale = reader.Reader.ReadSingle(); }

                int m_FontData_size = reader.Reader.ReadInt32();
                if (m_FontData_size > 0)
                {
                    m_FontData = reader.Reader.ReadBytes(m_FontData_size);
                }
            }
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (m_FontData is null)
            {
                return;
            }
            var extension = ".ttf";
            if (m_FontData[0] == 79 && m_FontData[1] == 84 && m_FontData[2] == 84 && m_FontData[3] == 79)
            {
                extension = ".otf";
            }
            if (!LocationStorage.TryCreate(fileName, extension, mode, out fileName))
            {
                return;
            }
            File.WriteAllBytes(fileName, m_FontData);
        }
    }
}
