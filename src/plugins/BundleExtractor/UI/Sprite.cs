using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.UI
{
    public class SecondarySpriteTexture
    {
        public PPtr<Texture2D> texture;
        public string name;

        public SecondarySpriteTexture(UIReader reader)
        {
            texture = new PPtr<Texture2D>(reader);
            name = reader.Reader.ReadStringZeroTerm();
        }
    }

    public enum SpritePackingRotation
    {
        None = 0,
        FlipHorizontal = 1,
        FlipVertical = 2,
        Rotate180 = 3,
        Rotate90 = 4
    };

    public enum SpritePackingMode
    {
        Tight = 0,
        Rectangle
    };

    public enum SpriteMeshType
    {
        FullRect,
        Tight
    };

    public class SpriteSettings
    {
        public uint settingsRaw;

        public uint packed;
        public SpritePackingMode packingMode;
        public SpritePackingRotation packingRotation;
        public SpriteMeshType meshType;

        public SpriteSettings(BinaryReader reader)
        {
            settingsRaw = reader.ReadUInt32();

            packed = settingsRaw & 1; //1
            packingMode = (SpritePackingMode)((settingsRaw >> 1) & 1); //1
            packingRotation = (SpritePackingRotation)((settingsRaw >> 2) & 0xf); //4
            meshType = (SpriteMeshType)((settingsRaw >> 6) & 1); //1
            //reserved
        }
    }

    public class SpriteVertex
    {
        public Vector3 pos;
        public Vector2 uv;

        public SpriteVertex(UIReader reader)
        {
            var version = reader.Version;

            pos = reader.ReadVector3();
            if (version.GreaterThanOrEquals(4,3)) //4.3 and down
            {
                uv = reader.ReadVector2();
            }
        }
    }

    public class SpriteRenderData
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
                var secondaryTexturesSize = reader.Reader.ReadInt32();
                secondaryTextures = new List<SecondarySpriteTexture>();
                for (int i = 0; i < secondaryTexturesSize; i++)
                {
                    secondaryTextures.Add(new SecondarySpriteTexture(reader));
                }
            }

            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var m_SubMeshesSize = reader.Reader.ReadInt32();
                m_SubMeshes = new List<SubMesh>();
                for (int i = 0; i < m_SubMeshesSize; i++)
                {
                    m_SubMeshes.Add(new SubMesh(reader));
                }

                m_IndexBuffer = reader.Reader.ReadArray(r => r.ReadByte());
                reader.Reader.AlignStream();

                m_VertexData = new VertexData(reader);
            }
            else
            {
                var verticesSize = reader.Reader.ReadInt32();
                vertices = new List<SpriteVertex>();
                for (int i = 0; i < verticesSize; i++)
                {
                    vertices.Add(new SpriteVertex(reader));
                }

                indices = reader.Reader.ReadArray(r => r.ReadUInt16());
                reader.Reader.AlignStream();
            }

            if (version.GreaterThanOrEquals(2018)) //2018 and up
            {
                m_Bindpose = reader.ReadMatrixArray();

                if (version.Major == 2018 && version.Minor < 2) //2018.2 down
                {
                    var m_SourceSkinSize = reader.Reader.ReadInt32();
                    for (int i = 0; i < m_SourceSkinSize; i++)
                    {
                        m_SourceSkin[i] = new BoneWeights4(reader);
                    }
                }
            }

            textureRect = new Rectf(reader.Reader);
            textureRectOffset = reader.ReadVector2();
            if (version.GreaterThanOrEquals(5,6)) //5.6 and up
            {
                atlasRectOffset = reader.ReadVector2();
            }

            settingsRaw = new SpriteSettings(reader.Reader);
            if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
            {
                uvTransform = reader.ReadVector4();
            }

            if (version.GreaterThanOrEquals(2017)) //2017 and up
            {
                downscaleMultiplier = reader.Reader.ReadSingle();
            }
        }
    }

    public class Rectf
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rectf(BinaryReader reader)
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            width = reader.ReadSingle();
            height = reader.ReadSingle();
        }
    }

    public sealed class Sprite : NamedObject, IFileWriter
    {
        public Rectf m_Rect;
        public Vector2 m_Offset;
        public Vector4 m_Border;
        public float m_PixelsToUnits;
        public Vector2 m_Pivot = new Vector2(0.5f, 0.5f);
        public uint m_Extrude;
        public bool m_IsPolygon;
        public KeyValuePair<Guid, long> m_RenderDataKey;
        public string[] m_AtlasTags;
        public PPtr<SpriteAtlas> m_SpriteAtlas;
        public SpriteRenderData m_RD;
        public List<Vector2[]> m_PhysicsShape;

        public Sprite(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            m_Rect = new Rectf(reader.Reader);
            m_Offset = reader.ReadVector2();
            if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
            {
                m_Border = reader.ReadVector4();
            }

            m_PixelsToUnits = reader.Reader.ReadSingle();
            if (version.GreaterThanOrEquals(5, 4, 1, Models.UnityVersionType.Patch, 3)) //5.4.1p3 and up
            {
                m_Pivot = reader.ReadVector2();
            }

            m_Extrude = reader.Reader.ReadUInt32();
            if (version.GreaterThanOrEquals(5, 3)) //5.3 and up
            {
                m_IsPolygon = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();
            }

            if (version.GreaterThanOrEquals(2017)) //2017 and up
            {
                var first = new Guid(reader.Reader.ReadBytes(16));
                var second = reader.Reader.ReadInt64();
                m_RenderDataKey = new KeyValuePair<Guid, long>(first, second);

                m_AtlasTags = reader.Reader.ReadArray(r => r.ReadString());

                m_SpriteAtlas = new PPtr<SpriteAtlas>(reader);
            }

            m_RD = new SpriteRenderData(reader);

            if (version.GreaterThanOrEquals(2017)) //2017 and up
            {
                var m_PhysicsShapeSize = reader.Reader.ReadInt32();
                m_PhysicsShape = new List<Vector2[]>();
                for (int i = 0; i < m_PhysicsShapeSize; i++)
                {
                    m_PhysicsShape.Add(reader.Reader.ReadArray(_ => reader.ReadVector2()));
                }
            }

            //vector m_Bones 2018 and up
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".png", mode, out fileName))
            {
                return;
            }
            GetImage(this)?.SaveAs(fileName);
        }



        #region CutImage
        public static SKImage? GetImage(Sprite m_Sprite)
        {
            if (m_Sprite.m_SpriteAtlas != null && m_Sprite.m_SpriteAtlas.TryGet(out var m_SpriteAtlas))
            {
                if (m_SpriteAtlas.m_RenderDataMap.TryGetValue(m_Sprite.m_RenderDataKey, out var spriteAtlasData) && spriteAtlasData.texture.TryGet(out var m_Texture2D))
                {
                    return CutImage(m_Sprite, m_Texture2D, spriteAtlasData.textureRect, spriteAtlasData.textureRectOffset, spriteAtlasData.downscaleMultiplier, spriteAtlasData.settingsRaw);
                }
            }
            else
            {
                if (m_Sprite.m_RD.texture.TryGet(out var m_Texture2D))
                {
                    return CutImage(m_Sprite, m_Texture2D, m_Sprite.m_RD.textureRect, m_Sprite.m_RD.textureRectOffset, m_Sprite.m_RD.downscaleMultiplier, m_Sprite.m_RD.settingsRaw);
                }
            }
            return null;
        }

        private static SKImage? CutImage(Sprite m_Sprite, Texture2D m_Texture2D, Rectf textureRect, Vector2 textureRectOffset, float downscaleMultiplier, SpriteSettings settingsRaw)
        {
            var originalImage = m_Texture2D.ToImage();
            if (originalImage != null)
            {
                using (originalImage)
                {
                    if (downscaleMultiplier > 0f && downscaleMultiplier != 1f)
                    {
                        var width = (int)(m_Texture2D.m_Width / downscaleMultiplier);
                        var height = (int)(m_Texture2D.m_Height / downscaleMultiplier);
                        originalImage = originalImage.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
                    }
                    var rectX = (int)Math.Floor(textureRect.x);
                    var rectY = (int)Math.Floor(textureRect.y);
                    var rectRight = (int)Math.Ceiling(textureRect.x + textureRect.width);
                    var rectBottom = (int)Math.Ceiling(textureRect.y + textureRect.height);
                    rectRight = Math.Min(rectRight, originalImage.Width);
                    rectBottom = Math.Min(rectBottom, originalImage.Height);
                    var rect = new SKRectI(rectX, rectY, rectRight - rectX, rectBottom - rectY);
                    var spriteImage = originalImage.Clip(rect);
                    if (settingsRaw.packed == 1)
                    {
                        //RotateAndFlip
                        switch (settingsRaw.packingRotation)
                        {
                            case SpritePackingRotation.FlipHorizontal:
                                spriteImage = spriteImage.Flip();
                                break;
                            case SpritePackingRotation.FlipVertical:
                                spriteImage = spriteImage.Flip(false);
                                break;
                            case SpritePackingRotation.Rotate180:
                                spriteImage = spriteImage.Rotate(180);
                                break;
                            case SpritePackingRotation.Rotate90:
                                spriteImage.Rotate(270);
                                break;
                        }
                    }

                    //Tight
                    if (settingsRaw.packingMode == SpritePackingMode.Tight)
                    {
                        try
                        {
                            var triangles = GetTriangles(m_Sprite.m_RD);
                            var path = new SKPath();
                            foreach (var item in triangles)
                            {
                                path.AddPoly(item);
                            }
                            var matrix = Matrix3x2.CreateScale(m_Sprite.m_PixelsToUnits);
                            matrix *= Matrix3x2.CreateTranslation(m_Sprite.m_Rect.width * m_Sprite.m_Pivot.X - textureRectOffset.X, m_Sprite.m_Rect.height * m_Sprite.m_Pivot.Y - textureRectOffset.Y);
                            path.Transform(matrix.AsMatrix());
                            return spriteImage.Clip(path);
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    //Rectangle
                    return spriteImage.Flip(false);
                }
            }

            return null;
        }

        private static SKPoint[][] GetTriangles(SpriteRenderData m_RD)
        {
            if (m_RD.vertices != null) //5.6 down
            {
                var vertices = m_RD.vertices.Select(x => new SKPoint(x.pos.X, x.pos.Y)).ToArray();
                var triangleCount = m_RD.indices.Length / 3;
                var triangles = new SKPoint[triangleCount][];
                for (int i = 0; i < triangleCount; i++)
                {
                    var first = m_RD.indices[i * 3];
                    var second = m_RD.indices[i * 3 + 1];
                    var third = m_RD.indices[i * 3 + 2];
                    var triangle = new[] { vertices[first], vertices[second], vertices[third] };
                    triangles[i] = triangle;
                }
                return triangles;
            }
            else //5.6 and up
            {
                var triangles = new List<SKPoint[]>();
                var m_VertexData = m_RD.m_VertexData;
                var m_Channel = m_VertexData.m_Channels[0]; //kShaderChannelVertex
                var m_Stream = m_VertexData.m_Streams[m_Channel.stream];
                using (var vertexReader = new EndianReader(new MemoryStream(m_VertexData.m_DataSize), EndianType.LittleEndian))
                {
                    using (var indexReader = new EndianReader(new MemoryStream(m_RD.m_IndexBuffer), EndianType.LittleEndian))
                    {
                        foreach (var subMesh in m_RD.m_SubMeshes)
                        {
                            vertexReader.BaseStream.Position = m_Stream.offset + subMesh.firstVertex * m_Stream.stride + m_Channel.offset;

                            var vertices = new SKPoint[subMesh.vertexCount];
                            for (int v = 0; v < subMesh.vertexCount; v++)
                            {
                                vertices[v] = new SKPoint(vertexReader.ReadSingle(), vertexReader.ReadSingle()); vertexReader.ReadSingle();
                                vertexReader.BaseStream.Position += m_Stream.stride - 12;
                            }

                            indexReader.BaseStream.Position = subMesh.firstByte;

                            var triangleCount = subMesh.indexCount / 3u;
                            for (int i = 0; i < triangleCount; i++)
                            {
                                var first = indexReader.ReadUInt16() - subMesh.firstVertex;
                                var second = indexReader.ReadUInt16() - subMesh.firstVertex;
                                var third = indexReader.ReadUInt16() - subMesh.firstVertex;
                                var triangle = new[] { vertices[first], vertices[second], vertices[third] };
                                triangles.Add(triangle);
                            }
                        }
                    }
                }
                return triangles.ToArray();
            }
        }
        #endregion
    }
}
