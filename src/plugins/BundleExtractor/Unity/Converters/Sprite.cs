using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class SpriteConverter : BundleConverter<Sprite>, IBundleExporter
    {
        public override Sprite? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new Sprite
            {
                Name = reader.ReadAlignedString(),
                Rect = reader.ReadVector4(),
                Offset = reader.ReadVector2()
            };
            if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
            {
                res.Border = reader.ReadVector4();
            }

            res.PixelsToUnits = reader.ReadSingle();
            if (version.GreaterThanOrEquals(5, 4, 1, VersionType.Patch, 3)) //5.4.1p3 and up
            {
                res.Pivot = reader.ReadVector2();
            }

            res.Extrude = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(5, 3)) //5.3 and up
            {
                res.IsPolygon = reader.ReadBoolean();
                reader.AlignStream();
            }

            if (version.GreaterThanOrEquals(2017)) //2017 and up
            {
                var first = new Guid(reader.ReadBytes(16));
                var second = reader.ReadInt64();
                res.RenderDataKey = new KeyValuePair<Guid, long>(first, second);

                res.AtlasTags = reader.ReadArray(r => r.ReadAlignedString());

                res.SpriteAtlas = reader.ReadPPtr<SpriteAtlas>(serializer);
            }

            res.RD = serializer.Deserialize<SpriteRenderData>(reader);

            if (version.GreaterThanOrEquals(2017)) //2017 and up
            {
                res.PhysicsShape = reader.ReadArray(_ => reader.ReadArray(_ => reader.ReadVector2()));
            }

            //vector m_Bones 2018 and up
            
            if (reader.TryGet<IDependencyBuilder>(out var builder))
            {
                var container = reader.Get<ISerializedFile>();
                var fileName = container.FullPath;
                var fileId = reader.Get<ObjectInfo>().FileID;
                if (res.SpriteAtlas != null)
                {
                    builder?.AddDependencyEntry(fileName,
                        fileId,
                        res.SpriteAtlas.PathID);
                }
                else
                {
                    builder?.AddDependencyEntry(fileName,
                        fileId, res.RD.Texture.PathID);
                }
            }
            return res;
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".s.png", mode, out fileName))
            {
                return;
            }
            GetImage(this)?.SaveAs(fileName);
        }

        #region CutImage
        public static SKImage? GetImage(Sprite m_Sprite)
        {
            if (m_Sprite.SpriteAtlas != null && m_Sprite.SpriteAtlas.TryGet(out var m_SpriteAtlas))
            {
                if (m_SpriteAtlas.RenderDataMap.TryGetValue(m_Sprite.RenderDataKey, out var spriteAtlasData) && spriteAtlasData.Texture.TryGet(out var m_Texture2D))
                {
                    return CutImage(m_Sprite, m_Texture2D, spriteAtlasData.TextureRect, spriteAtlasData.TextureRectOffset, spriteAtlasData.DownscaleMultiplier, spriteAtlasData.SettingsRaw);
                }
            }
            else
            {
                if (m_Sprite.RD.Texture.TryGet(out var m_Texture2D))
                {
                    return CutImage(m_Sprite, m_Texture2D, m_Sprite.RD.TextureRect, m_Sprite.RD.TextureRectOffset, m_Sprite.RD.DownscaleMultiplier, m_Sprite.RD.SettingsRaw);
                }
            }
            return null;
        }

        private static SKImage? CutImage(Sprite m_Sprite, Texture2D m_Texture2D, Vector4 textureRect, Vector2 textureRectOffset, float downscaleMultiplier, SpriteSettings settingsRaw)
        {
            var originalImage = m_Texture2D.ToImage();
            if (originalImage == null)
            {
                return null;
            }
            var isUpdated = false;
            SKImage? temp;
            if (downscaleMultiplier > 0f && downscaleMultiplier != 1f)
            {
                var width = (int)(m_Texture2D.Width / downscaleMultiplier);
                var height = (int)(m_Texture2D.Height / downscaleMultiplier);
                temp = originalImage.Resize(new SKImageInfo(width, height), SKSamplingOptions.Default);
                originalImage.Dispose();
                originalImage = temp;
                isUpdated = true;
            }
            if (originalImage is null)
            {
                return null;
            }
            var rectX = (int)Math.Floor(textureRect.X);
            var rectY = (int)Math.Floor(textureRect.Y);
            var rectRight = (int)Math.Ceiling(textureRect.X + textureRect.Z);
            var rectBottom = (int)Math.Ceiling(textureRect.Y + textureRect.W);
            rectRight = Math.Min(rectRight, originalImage.Width);
            rectBottom = Math.Min(rectBottom, originalImage.Height);
            if (rectX != 0 || rectY != 0 || rectRight != originalImage.Width 
                || rectBottom != originalImage.Height)
            {
                var rect = new SKRectI(rectX, rectY, rectRight, rectBottom);
                temp = originalImage.Subset(rect);
                originalImage.Dispose();
                originalImage = temp;
                isUpdated = true;
            }
            
            if (settingsRaw.Packed == 1)
            {
                //RotateAndFlip
                switch (settingsRaw.PackingRotation)
                {
                    case SpritePackingRotation.FlipHorizontal:
                        temp = originalImage.Flip();
                        break;
                    case SpritePackingRotation.FlipVertical:
                        temp = originalImage.Flip(false);
                        break;
                    case SpritePackingRotation.Rotate180:
                        temp = originalImage.Rotate(180);
                        break;
                    case SpritePackingRotation.Rotate90:
                        temp = originalImage.Rotate(270);
                        break;
                    default:
                        temp = null;
                        break;
                }
                if (temp is not null)
                {
                    isUpdated = true;
                    originalImage.Dispose();
                    originalImage = temp;
                }
            }

            //Tight
            if (settingsRaw.PackingMode == SpritePackingMode.Tight)
            {
                try
                {
                    var triangles = GetTriangles(m_Sprite.RD);
                    var path = new SKPath();
                    var matrix = Matrix3x2.CreateScale(m_Sprite.PixelsToUnits);
                    matrix *= Matrix3x2.CreateTranslation(m_Sprite.Rect.Z * m_Sprite.Pivot.X - textureRectOffset.X, m_Sprite.Rect.W * m_Sprite.Pivot.Y - textureRectOffset.Y);
                    foreach (var item in triangles)
                    {
                        path.AddPoly(item, true);
                    }
                    path.Transform(matrix.AsMatrix());
                    temp = originalImage?.ClipAndFlip(path, false);
                    originalImage?.Dispose();
                    return temp;
                }
                catch
                {
                    // ignored
                }
            }

            if (isUpdated)
            {
                temp = originalImage.Flip(false);
                originalImage?.Dispose();
                return temp;
            }
            originalImage?.Dispose();
            return null;
        }

        private static SKPoint[][] GetTriangles(SpriteRenderData m_RD)
        {
            if (m_RD.Vertices != null) //5.6 down
            {
                var vertices = m_RD.Vertices.Select(x => new SKPoint(x.Pos.X, x.Pos.Y)).ToArray();
                var triangleCount = m_RD.Indices.Length / 3;
                var triangles = new SKPoint[triangleCount][];
                for (int i = 0; i < triangleCount; i++)
                {
                    var first = m_RD.Indices[i * 3];
                    var second = m_RD.Indices[i * 3 + 1];
                    var third = m_RD.Indices[i * 3 + 2];
                    triangles[i] = [vertices[first], vertices[second], vertices[third]];
                }
                return triangles;
            }
            else //5.6 and up
            {
                var triangles = new List<SKPoint[]>();
                var m_VertexData = m_RD.VertexData;
                var m_Channel = m_VertexData.Channels[0]; //kShaderChannelVertex
                var m_Stream = m_VertexData.Streams[m_Channel.Stream];
                using (var vertexReader = new EndianReader(new MemoryStream(m_VertexData.DataSize), EndianType.LittleEndian))
                {
                    using (var indexReader = new EndianReader(new MemoryStream(m_RD.IndexBuffer), EndianType.LittleEndian))
                    {
                        foreach (var subMesh in m_RD.SubMeshes)
                        {
                            vertexReader.BaseStream.Position = m_Stream.Offset + subMesh.FirstVertex * m_Stream.Stride + m_Channel.Offset;

                            var vertices = new SKPoint[subMesh.VertexCount];
                            for (int v = 0; v < subMesh.VertexCount; v++)
                            {
                                vertices[v] = new SKPoint(vertexReader.ReadSingle(), vertexReader.ReadSingle()); vertexReader.ReadSingle();
                                vertexReader.BaseStream.Position += m_Stream.Stride - 12;
                            }

                            indexReader.BaseStream.Position = subMesh.FirstByte;

                            var triangleCount = subMesh.IndexCount / 3u;
                            for (int i = 0; i < triangleCount; i++)
                            {
                                var first = indexReader.ReadUInt16() - subMesh.FirstVertex;
                                var second = indexReader.ReadUInt16() - subMesh.FirstVertex;
                                var third = indexReader.ReadUInt16() - subMesh.FirstVertex;
                                triangles.Add([vertices[first], vertices[second], vertices[third]]);
                            }
                        }
                    }
                }
                return [.. triangles];
            }
        }
        #endregion
    }
}
