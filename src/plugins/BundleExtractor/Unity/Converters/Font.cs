using System;
using System.IO;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class FontConverter : BundleConverter<Font>, IBundleExporter
    {
        public override Font? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new Font
            {
                Name = reader.ReadAlignedString()
            };
            if (version.GreaterThanOrEquals(5, 5))//5.5 and up
            {
                var m_LineSpacing = reader.ReadSingle();
                var m_DefaultMaterial = reader.ReadPPtr<Material>(serializer);
                var m_FontSize = reader.ReadSingle();
                var m_Texture = reader.ReadPPtr<Texture>(serializer);
                int m_AsciiStartOffset = reader.ReadInt32();
                var m_Tracking = reader.ReadSingle();
                var m_CharacterSpacing = reader.ReadInt32();
                var m_CharacterPadding = reader.ReadInt32();
                var m_ConvertCase = reader.ReadInt32();
                int m_CharacterRects_size = reader.ReadInt32();
                for (int i = 0; i < m_CharacterRects_size; i++)
                {
                    reader.Position += 44;//CharacterInfo data 41
                }
                int m_KerningValues_size = reader.ReadInt32();
                for (int i = 0; i < m_KerningValues_size; i++)
                {
                    reader.Position += 8;
                }
                var m_PixelScale = reader.ReadSingle();
                int m_FontData_size = reader.ReadInt32();
                if (m_FontData_size > 0)
                {
                    res.FontData = reader.ReadBytes(m_FontData_size);
                }
            }
            else
            {
                int m_AsciiStartOffset = reader.ReadInt32();

                if (version.LessThanOrEquals(3))
                {
                    int m_FontCountX = reader.ReadInt32();
                    int m_FontCountY = reader.ReadInt32();
                }

                float m_Kerning = reader.ReadSingle();
                float m_LineSpacing = reader.ReadSingle();

                if (version.LessThanOrEquals(3))
                {
                    int m_PerCharacterKerning_size = reader.ReadInt32();
                    for (int i = 0; i < m_PerCharacterKerning_size; i++)
                    {
                        int first = reader.ReadInt32();
                        float second = reader.ReadSingle();
                    }
                }
                else
                {
                    int m_CharacterSpacing = reader.ReadInt32();
                    int m_CharacterPadding = reader.ReadInt32();
                }

                int m_ConvertCase = reader.ReadInt32();
                var m_DefaultMaterial = reader.ReadPPtr<Material>(serializer);

                int m_CharacterRects_size = reader.ReadInt32();
                for (int i = 0; i < m_CharacterRects_size; i++)
                {
                    int index = reader.ReadInt32();
                    //Rectf uv
                    float uvx = reader.ReadSingle();
                    float uvy = reader.ReadSingle();
                    float uvwidth = reader.ReadSingle();
                    float uvheight = reader.ReadSingle();
                    //Rectf vert
                    float vertx = reader.ReadSingle();
                    float verty = reader.ReadSingle();
                    float vertwidth = reader.ReadSingle();
                    float vertheight = reader.ReadSingle();
                    float width = reader.ReadSingle();

                    if (version.GreaterThanOrEquals(4))
                    {
                        var flipped = reader.ReadBoolean();
                        reader.AlignStream();
                    }
                }

                var m_Texture = reader.ReadPPtr<Texture>(serializer);

                int m_KerningValues_size = reader.ReadInt32();
                for (int i = 0; i < m_KerningValues_size; i++)
                {
                    int pairfirst = reader.ReadInt16();
                    int pairsecond = reader.ReadInt16();
                    float second = reader.ReadSingle();
                }

                if (version.LessThanOrEquals(3))
                {
                    var m_GridFont = reader.ReadBoolean();
                    reader.AlignStream();
                }
                else { float m_PixelScale = reader.ReadSingle(); }

                int m_FontData_size = reader.ReadInt32();
                if (m_FontData_size > 0)
                {
                    res.FontData = reader.ReadBytes(m_FontData_size);
                }
            }
            return res;
        }

        public void SaveAs(Font res, string fileName, ArchiveExtractMode mode)
        {
            if (res.FontData is null)
            {
                return;
            }
            var extension = ".ttf";
            if (res.FontData.StartsWith([79, 84, 84, 79]))
            {
                extension = ".otf";
            }
            if (!LocationStorage.TryCreate(fileName, extension, mode, out fileName))
            {
                return;
            }
            File.WriteAllBytes(fileName, res.FontData);
        }
    }
}
