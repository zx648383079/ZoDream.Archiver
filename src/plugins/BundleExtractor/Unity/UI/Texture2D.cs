using SkiaSharp;
using System.Buffers;
using System.IO;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.UI
{
    

    internal class GLTextureSettings : IElementLoader
    {
        /// <summary>
        /// point Bilinear Trilinear
        /// </summary>
        public int m_FilterMode;
        public int m_Aniso;
        public float m_MipBias;
        public int m_WrapMode;
        public int m_WrapV;
        public int m_WrapW;



        public void Read(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            m_FilterMode = reader.ReadInt32();
            m_Aniso = reader.ReadInt32();
            m_MipBias = reader.ReadSingle();
            if (version.Major >= 2017)//2017.x and up
            {
                m_WrapMode = reader.ReadInt32(); //m_WrapU
                m_WrapV = reader.ReadInt32();
                m_WrapW = reader.ReadInt32();
            }
            else
            {
                m_WrapMode = reader.ReadInt32();
            }
        }
    }

    internal sealed class Texture2D(UIReader reader) : 
        Texture(reader), IFileExporter, IElementTypeLoader
    {
        public int m_Width;
        public int m_Height;
        public TextureFormat m_TextureFormat;
        public bool m_MipMap;
        public int m_MipCount;
        public GLTextureSettings m_TextureSettings;
        public Stream image_data;
        public StreamingInfo m_StreamData;


        public void Read(IBundleBinaryReader reader, TypeTree typeMaps)
        {
            TypeTreeHelper.ReadType(typeMaps, reader, this);
            if (!string.IsNullOrEmpty(m_StreamData?.path))
            {
                image_data = _reader.OpenResource(m_StreamData);
            }
        }

        public void ReadBase(IBundleBinaryReader reader)
        {
            base.Read(reader);
        }

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var version = reader.Get<UnityVersion>();
            m_Width = reader.ReadInt32();
            m_Height = reader.ReadInt32();
            var m_CompleteImageSize = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020)) //2020.1 and up
            {
                var m_MipsStripped = reader.ReadInt32();
            }
            m_TextureFormat = (TextureFormat)reader.ReadInt32();
            if (version.LessThan(5, 2)) //5.2 down
            {
                m_MipMap = reader.ReadBoolean();
            }
            else
            {
                m_MipCount = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and up
            {
                var m_IsReadable = reader.ReadBoolean();
            
            }
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                var m_IsPreProcessed = reader.ReadBoolean();
            }
            if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
            {
                var m_IgnoreMasterTextureLimit = reader.ReadBoolean();
            }
            if (version.GreaterThanOrEquals(2022, 2)) //2022.2 and up
            {
                reader.AlignStream(); //m_IgnoreMipmapLimit
                var m_MipmapLimitGroupName = reader.ReadAlignedString();
            }
            if (version.GreaterThanOrEquals(3)) //3.0.0 - 5.4
            {
                if (version.LessThanOrEquals(5, 4))
                {
                    var m_ReadAllowed = reader.ReadBoolean();
                }
            }
            if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
            {
                var m_StreamingMipmaps = reader.ReadBoolean();
            }
            reader.AlignStream();
            if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
            {
                var m_StreamingMipmapsPriority = reader.ReadInt32();
            }
            var m_ImageCount = reader.ReadInt32();
            var m_TextureDimension = reader.ReadInt32();
            m_TextureSettings = new GLTextureSettings();
            reader.Get<IBundleElementScanner>().TryRead(reader, m_TextureSettings);
            if (version.GreaterThanOrEquals(3)) //3.0 and up
            {
                var m_LightmapFormat = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(3, 5)) //3.5.0 and up
            {
                var m_ColorSpace = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                var m_PlatformBlob = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
            }
            var image_data_size = reader.ReadInt32();
            if (image_data_size == 0 && version.GreaterThanOrEquals(5, 3))//5.3.0 and up
            {
                m_StreamData = new StreamingInfo(reader);
            }

            if (!string.IsNullOrEmpty(m_StreamData?.path))
            {
                image_data = _reader.OpenResource(m_StreamData);
            } else
            {
                image_data = new PartialStream(reader.BaseStream, image_data_size);
            }
        }


        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".png", mode, out fileName))
            {
                return;
            }
            using var image = ToImage();
            image?.Flip(false).SaveAs(fileName);
        }

        public SKImage? ToImage()
        {
            if (image_data is null)
            {
                return null;
            }
            image_data.Position = 0;
            var data = TextureExtension.Decode(image_data.ToArray(), m_Width,
                m_Height, m_TextureFormat, _reader.Version);
            return data?.ToImage();
        }
    }

    
}