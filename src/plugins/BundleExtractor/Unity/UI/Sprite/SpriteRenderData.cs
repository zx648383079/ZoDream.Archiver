using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SpriteRenderData
    {
        public PPtr<Texture2D> texture;
        public PPtr<Texture2D> alphaTexture;
        public List<SecondarySpriteTexture> secondaryTextures;
        public List<SubMesh> m_SubMeshes;
        public byte[] m_IndexBuffer;
        public VertexData m_VertexData;
        public List<SpriteVertex> vertices;
        public ushort[] indices;
        public Matrix4x4[] m_Bindpose;
        public List<BoneWeights4> m_SourceSkin;
        public Rectf textureRect;
        public Vector2 textureRectOffset;
        public Vector2 atlasRectOffset;
        public SpriteSettings settingsRaw;
        public Vector4 uvTransform;
        public float downscaleMultiplier;

        public SpriteRenderData(UIReader reader)
        {
            var version = reader.Version;

            texture = new PPtr<Texture2D>(reader);
            if (version.GreaterThanOrEquals(5, 2)) //5.2 and up
            {
                alphaTexture = new PPtr<Texture2D>(reader);
            }

            if (version.GreaterThanOrEquals(2019)) //2019 and up
            {
                var secondaryTexturesSize = reader.ReadInt32();
                secondaryTextures = new List<SecondarySpriteTexture>();
                for (int i = 0; i < secondaryTexturesSize; i++)
                {
                    secondaryTextures.Add(new SecondarySpriteTexture(reader));
                }
            }

            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var m_SubMeshesSize = reader.ReadInt32();
                m_SubMeshes = new List<SubMesh>();
                for (int i = 0; i < m_SubMeshesSize; i++)
                {
                    m_SubMeshes.Add(new SubMesh(reader));
                }

                m_IndexBuffer = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();

                m_VertexData = new VertexData(reader);
            }
            else
            {
                var verticesSize = reader.ReadInt32();
                vertices = new List<SpriteVertex>();
                for (int i = 0; i < verticesSize; i++)
                {
                    vertices.Add(new SpriteVertex(reader));
                }

                indices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
            }

            if (version.GreaterThanOrEquals(2018)) //2018 and up
            {
                m_Bindpose = reader.ReadMatrixArray();

                if (version.Major == 2018 && version.Minor < 2) //2018.2 down
                {
                    var m_SourceSkinSize = reader.ReadInt32();
                    for (int i = 0; i < m_SourceSkinSize; i++)
                    {
                        m_SourceSkin[i] = new BoneWeights4(reader);
                    }
                }
            }

            textureRect = new Rectf(reader);
            textureRectOffset = reader.ReadVector2();
            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                atlasRectOffset = reader.ReadVector2();
            }

            settingsRaw = new SpriteSettings(reader);
            if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
            {
                uvTransform = reader.ReadVector4();
            }

            if (version.GreaterThanOrEquals(2017)) //2017 and up
            {
                downscaleMultiplier = reader.ReadSingle();
            }
        }
    }

}
