using System.Collections.Generic;
using System.IO;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class Texture2DArray(UIReader reader) : Texture(reader)
    {
        public int m_Width;
        public int m_Height;
        public int m_Depth;
        public GraphicsFormat m_Format;
        public int m_MipCount;
        public uint m_DataSize;
        public GLTextureSettings m_TextureSettings;
        public int m_ColorSpace;
        public Stream image_data;
        public StreamingInfo m_StreamData;
        public List<Texture2D> TextureList;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_ColorSpace = reader.ReadInt32();
            m_Format = (GraphicsFormat)reader.ReadInt32();
            m_Width = reader.ReadInt32();
            m_Height = reader.ReadInt32();
            m_Depth = reader.ReadInt32();
            m_MipCount = reader.ReadInt32();
            m_DataSize = reader.ReadUInt32();
            m_TextureSettings = new GLTextureSettings();
            reader.Get<IBundleElementScanner>().TryRead(reader, m_TextureSettings);
            if (reader.Get<UnityVersion>().GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                var m_UsageMode = reader.ReadInt32();
            }
            var m_IsReadable = reader.ReadBoolean();
            reader.AlignStream();

            var image_data_size = reader.ReadInt32();
            if (image_data_size == 0)
            {
                m_StreamData = new StreamingInfo(reader);
            }

            if (!string.IsNullOrEmpty(m_StreamData?.path))
            {
                image_data = _reader.OpenResource(m_StreamData);
            }
            else
            {
                image_data = new PartialStream(reader.BaseStream, image_data_size);
            }

            TextureList = [];
        }

        public override void Associated(IDependencyBuilder? builder)
        {
            base.Associated(builder);
            if (!string.IsNullOrEmpty(m_StreamData?.path))
            {
                builder?.AddDependencyEntry(_reader.FullPath, FileID, m_StreamData.path);
            }
        }
    }
}
