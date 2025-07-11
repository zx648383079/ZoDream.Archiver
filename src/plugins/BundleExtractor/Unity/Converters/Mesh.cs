using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    
    internal class MeshConverter : BundleConverter<Mesh>
    {
        public override Mesh? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Mesh();
            var version = reader.Get<Version>();
            ReadBase(res, reader, serializer, () => {
                if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and up
                {
                    if (version.GreaterThanOrEquals(2019)) //2019 and up
                    {
                        var m_BonesAABB = reader.ReadArray<MinMaxAABB>(serializer);
            

                        var m_VariableBoneCountWeights = reader.ReadArray(r => r.ReadUInt32());
                    }

                    var m_MeshCompression = reader.ReadByte();
                    if (version.GreaterThanOrEquals(4))
                    {
                        if (version.LessThan(5))
                        {
                            var m_StreamCompression = reader.ReadByte();
                        }
                        var m_IsReadable = reader.ReadBoolean();
                        
                        var m_KeepVertices = reader.ReadBoolean();
                        var m_KeepIndices = reader.ReadBoolean();
                        
                    }
                    reader.AlignStream();

                    //Unity fixed it in 2017.3.1p1 and later versions
                    if (
                        version.GreaterThanOrEquals(2017, 4) ||
                        version.Equals(2017, 3, 1, VersionType.Patch, 1) ||
                        (version.Equals(2017, 3) && m_MeshCompression == 0)
                    )//2017.3.xfx with no compression
                    {
                        var m_IndexFormat = reader.ReadInt32();
                        res.Use16BitIndices = m_IndexFormat == 0;
                    }

                    int m_IndexBuffer_size = reader.ReadInt32();
                    if (res.Use16BitIndices)
                    {
                        res.IndexBuffer = reader.ReadArray(m_IndexBuffer_size / 2, r => (uint)r.ReadUInt16());
                        reader.AlignStream();
                    }
                    else
                    {
                        res.IndexBuffer = reader.ReadArray(m_IndexBuffer_size / 4, r => r.ReadUInt32());
                    }
                }

                if (version.LessThan(3, 5)) //3.4.2 and earlier
                {
                    res.VertexCount = reader.ReadInt32();
                    res.Vertices = reader.ReadArray(res.VertexCount * 3, r => r.ReadSingle()); //Vector3

                    res.Skin = reader.ReadArray<BoneWeights4>(serializer);

                    res.BindPose = reader.ReadMatrixArray();

                    res.UV0 = reader.ReadArray(reader.ReadInt32() * 2, r => r.ReadSingle()); //Vector2

                    res.UV1 = reader.ReadArray(reader.ReadInt32() * 2, r => r.ReadSingle()); //Vector2

                    if (version.LessThanOrEquals(2, 5)) //2.5 and down
                    {
                        int m_TangentSpace_size = reader.ReadInt32();
                        res.Normals = new float[m_TangentSpace_size * 3];
                        res.Tangents = new float[m_TangentSpace_size * 4];
                        for (int v = 0; v < m_TangentSpace_size; v++)
                        {
                            res.Normals[v * 3] = reader.ReadSingle();
                            res.Normals[v * 3 + 1] = reader.ReadSingle();
                            res.Normals[v * 3 + 2] = reader.ReadSingle();
                            res.Tangents[v * 3] = reader.ReadSingle();
                            res.Tangents[v * 3 + 1] = reader.ReadSingle();
                            res.Tangents[v * 3 + 2] = reader.ReadSingle();
                            res.Tangents[v * 3 + 3] = reader.ReadSingle(); //handedness
                        }
                    }
                    else //2.6.0 and later
                    {
                        res.Tangents = reader.ReadArray(reader.ReadInt32() * 4, r => r.ReadSingle()); //Vector4

                        res.Normals = reader.ReadArray(reader.ReadInt32() * 3, r => r.ReadSingle()); //Vector3
                    }
                }
                else
                {
                    if (version.LessThan(2018, 2)) //2018.2 down
                    {
                        res.Skin = reader.ReadArray<BoneWeights4>(serializer);
                    }

                    if (version.LessThanOrEquals(4, 2)) //4.2 and down
                    {
                        res.BindPose = reader.ReadMatrixArray();
                    }

                    res.VertexData = serializer.Deserialize<VertexData>(reader);
                }

                if (version.GreaterThanOrEquals(2, 6) && !res.CollisionMeshBaked) //2.6.0 and later
                {
                    res.CompressedMesh = serializer.Deserialize<CompressedMesh>(reader);
                }

                reader.Position += 24; //AABB m_LocalAABB

                if (version.LessThanOrEquals(3, 4, 2)) //3.4.2 and earlier
                {
                    int m_Colors_size = reader.ReadInt32();
                    res.Colors = new float[m_Colors_size * 4];
                    for (int v = 0; v < m_Colors_size * 4; v++)
                    {
                        res.Colors[v] = (float)reader.ReadByte() / 0xFF;
                    }

                    int m_CollisionTriangles_size = reader.ReadInt32();
                    reader.Position += m_CollisionTriangles_size * 4; //UInt32 indices
                    int m_CollisionVertexCount = reader.ReadInt32();
                }


                int m_MeshUsageFlags = reader.ReadInt32();

                if (version.GreaterThanOrEquals(2022, 1)) //2022.1 and up
                {
                    int m_CookingOptions = reader.ReadInt32();
                }

                if (version.GreaterThanOrEquals(5)) //5.0 and up
                {
                    var m_BakedConvexCollisionMesh = reader.ReadArray(r => r.ReadByte());
                    reader.AlignStream();
                    var m_BakedTriangleCollisionMesh = reader.ReadArray(r => r.ReadByte());
                    reader.AlignStream();
                }


                if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
                {
                    var m_MeshMetrics = new float[2];
                    m_MeshMetrics[0] = reader.ReadSingle();
                    m_MeshMetrics[1] = reader.ReadSingle();
                }

            });
            return res;
        }

        public static void ReadBase(Mesh res, IBundleBinaryReader reader, IBundleSerializer serializer, Action cb)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            res.Name = reader.ReadAlignedString();
            if (version.LessThan(3, 5)) //3.5 down
            {
                res.Use16BitIndices = reader.ReadInt32() > 0;
            }

            if (version.LessThanOrEquals(2, 5)) //2.5 and down
            {
                int m_IndexBuffer_size = reader.ReadInt32();

                if (res.Use16BitIndices)
                {
                    res.IndexBuffer = new uint[m_IndexBuffer_size / 2];
                    for (int i = 0; i < m_IndexBuffer_size / 2; i++)
                    {
                        res.IndexBuffer[i] = reader.ReadUInt16();
                    }
                    reader.AlignStream();
                }
                else
                {
                    res.IndexBuffer = reader.ReadArray(m_IndexBuffer_size / 4, (r, _) => r.ReadUInt32());
                }
            }

            res.SubMeshes = reader.ReadArray<SubMesh>(serializer);

            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                res.Shapes = serializer.Deserialize<BlendShapeData>(reader);
            }

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.BindPose = reader.ReadMatrixArray();
                res.BoneNameHashes = reader.ReadArray(r => r.ReadUInt32());
                var m_RootBoneNameHash = reader.ReadUInt32();
            }

            cb.Invoke();

            

            if (version.GreaterThanOrEquals(2018, 3)) //2018.3 and up
            {
                reader.AlignStream();
                res.StreamData = serializer.Deserialize<ResourceSource>(reader);

                // var m_GenerateGeometryBuffer = reader.ReadBoolean();
                if (reader.TryGet<IDependencyBuilder>(out var builder))
                {
                    var container = reader.Get<ISerializedFile>();
                    var sourcePath = container.FullPath;
                    var fileId = reader.Get<ObjectInfo>().FileID;
                    builder.AddDependencyEntry(sourcePath,
                        fileId,
                        res.StreamData.Source);
                }
            }
            
            ProcessData(res, reader);
        }

        private static void ProcessData(Mesh res, IBundleBinaryReader reader)
        {
            var version = reader.Get<Version>();
            if (!string.IsNullOrEmpty(res.StreamData.Source))
            {
                if (res.VertexData.VertexCount > 0)
                {
                    res.VertexData.DataSize = reader.Get<ISerializedFile>().OpenResource(res.StreamData).ToArray();
                }
            }
            if (version.GreaterThanOrEquals(3, 5)) //3.5 and up
            {
                ReadVertexData(res, reader);
            }

            if (res.CollisionMeshBaked)
            {
                return;
            }

            if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and later
            {
                DecompressCompressedMesh(res, version);
            }

            GetTriangles(res, version);
        }

        private static void ReadVertexData(Mesh res, IBundleBinaryReader reader)
        {
            res.VertexCount = (int)res.VertexData.VertexCount;
            var version = reader.Get<Version>();
            for (var chn = 0; chn < res.VertexData.Channels.Length; chn++)
            {
                var m_Channel = res.VertexData.Channels[chn];
                if (m_Channel.Dimension > 0)
                {
                    var m_Stream = res.VertexData.Streams[m_Channel.Stream];
                    var channelMask = new BitArray([(int)m_Stream.ChannelMask]);
                    if (channelMask.Get(chn))
                    {
                        if (version.LessThan(2018) && chn == 2 && m_Channel.Format == 2) //kShaderChannelColor && kChannelFormatColor
                        {
                            m_Channel.Dimension = 4;
                        }

                        var vertexFormat = MeshHelper.ToVertexFormat(m_Channel.Format, version);
                        var componentByteSize = (int)MeshHelper.GetFormatSize(vertexFormat);
                        var componentBytes = new byte[res.VertexCount * m_Channel.Dimension * componentByteSize];
                        for (int v = 0; v < res.VertexCount; v++)
                        {
                            var vertexOffset = (int)m_Stream.Offset + m_Channel.Offset + (int)m_Stream.Stride * v;
                            for (int d = 0; d < m_Channel.Dimension; d++)
                            {
                                var componentOffset = vertexOffset + componentByteSize * d;
                                Buffer.BlockCopy(res.VertexData.DataSize, componentOffset, componentBytes, componentByteSize * (v * m_Channel.Dimension + d), componentByteSize);
                            }
                        }

                        if (reader.EndianType == EndianType.BigEndian && componentByteSize > 1) //swap bytes
                        {
                            for (var i = 0; i < componentBytes.Length / componentByteSize; i++)
                            {
                                var buff = new byte[componentByteSize];
                                Buffer.BlockCopy(componentBytes, i * componentByteSize, buff, 0, componentByteSize);
                                buff = buff.Reverse().ToArray();
                                Buffer.BlockCopy(buff, 0, componentBytes, i * componentByteSize, componentByteSize);
                            }
                        }

                        int[] componentsIntArray = null;
                        float[] componentsFloatArray = null;
                        if (MeshHelper.IsIntFormat(vertexFormat))
                            componentsIntArray = MeshHelper.BytesToIntArray(componentBytes, vertexFormat);
                        else
                            componentsFloatArray = MeshHelper.BytesToFloatArray(componentBytes, vertexFormat);

                        if (version.GreaterThanOrEquals(2018))
                        {
                            switch (chn)
                            {
                                case 0: //kShaderChannelVertex
                                    res.Vertices = componentsFloatArray;
                                    break;
                                case 1: //kShaderChannelNormal
                                    res.Normals = componentsFloatArray;
                                    break;
                                case 2: //kShaderChannelTangent
                                    res.Tangents = componentsFloatArray;
                                    break;
                                case 3: //kShaderChannelColor
                                    res.Colors = componentsFloatArray;
                                    break;
                                case 4: //kShaderChannelTexCoord0
                                    res.UV0 = componentsFloatArray;
                                    break;
                                case 5: //kShaderChannelTexCoord1
                                    res.UV1 = componentsFloatArray;
                                    break;
                                case 6: //kShaderChannelTexCoord2
                                    res.UV2 = componentsFloatArray;
                                    break;
                                case 7: //kShaderChannelTexCoord3
                                    res.UV3 = componentsFloatArray;
                                    break;
                                case 8: //kShaderChannelTexCoord4
                                    res.UV4 = componentsFloatArray;
                                    break;
                                case 9: //kShaderChannelTexCoord5
                                    res.UV5 = componentsFloatArray;
                                    break;
                                case 10: //kShaderChannelTexCoord6
                                    res.UV6 = componentsFloatArray;
                                    break;
                                case 11: //kShaderChannelTexCoord7
                                    res.UV7 = componentsFloatArray;
                                    break;
                                //2018.2 and up
                                case 12: //kShaderChannelBlendWeight
                                    if (res.Skin == null)
                                    {
                                        InitMSkin(res);
                                    }
                                    for (int i = 0; i < res.VertexCount; i++)
                                    {
                                        for (int j = 0; j < m_Channel.Dimension; j++)
                                        {
                                            res.Skin[i].Weight[j] = componentsFloatArray[i * m_Channel.Dimension + j];
                                        }
                                    }
                                    break;
                                case 13: //kShaderChannelBlendIndices
                                    if (res.Skin == null)
                                    {
                                        InitMSkin(res);
                                    }
                                    for (int i = 0; i < res.VertexCount; i++)
                                    {
                                        for (int j = 0; j < m_Channel.Dimension; j++)
                                        {
                                            res.Skin[i].BoneIndex[j] = componentsIntArray[i * m_Channel.Dimension + j];
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            switch (chn)
                            {
                                case 0: //kShaderChannelVertex
                                    res.Vertices = componentsFloatArray;
                                    break;
                                case 1: //kShaderChannelNormal
                                    res.Normals = componentsFloatArray;
                                    break;
                                case 2: //kShaderChannelColor
                                    res.Colors = componentsFloatArray;
                                    break;
                                case 3: //kShaderChannelTexCoord0
                                    res.UV0 = componentsFloatArray;
                                    break;
                                case 4: //kShaderChannelTexCoord1
                                    res.UV1 = componentsFloatArray;
                                    break;
                                case 5:
                                    if (version.Major >= 5) //kShaderChannelTexCoord2
                                    {
                                        res.UV2 = componentsFloatArray;
                                    }
                                    else //kShaderChannelTangent
                                    {
                                        res.Tangents = componentsFloatArray;
                                    }
                                    break;
                                case 6: //kShaderChannelTexCoord3
                                    res.UV3 = componentsFloatArray;
                                    break;
                                case 7: //kShaderChannelTangent
                                    res.Tangents = componentsFloatArray;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private static void DecompressCompressedMesh(Mesh res, Version version)
        {
            //Vertex
            if (res.CompressedMesh.Vertices.NumItems > 0)
            {
                res.VertexCount = (int)res.CompressedMesh.Vertices.NumItems / 3;
                res.Vertices = PackedFloatVectorConverter.UnpackFloats(res.CompressedMesh.Vertices, 3, 3 * 4);
            }
            //UV
            if (res.CompressedMesh.UV.NumItems > 0)
            {
                var m_UVInfo = res.CompressedMesh.UVInfo;
                if (m_UVInfo != 0)
                {
                    const int kInfoBitsPerUV = 4;
                    const int kUVDimensionMask = 3;
                    const int kUVChannelExists = 4;
                    const int kMaxTexCoordShaderChannels = 8;

                    int uvSrcOffset = 0;
                    for (int uv = 0; uv < kMaxTexCoordShaderChannels; uv++)
                    {
                        var texCoordBits = m_UVInfo >> uv * kInfoBitsPerUV;
                        texCoordBits &= (1u << kInfoBitsPerUV) - 1u;
                        if ((texCoordBits & kUVChannelExists) != 0)
                        {
                            var uvDim = 1 + (int)(texCoordBits & kUVDimensionMask);
                            var m_UV = PackedFloatVectorConverter.UnpackFloats(res.CompressedMesh.UV, uvDim, uvDim * 4, uvSrcOffset, res.VertexCount);
                            SetUV(res, uv, m_UV);
                            uvSrcOffset += uvDim * res.VertexCount;
                        }
                    }
                }
                else
                {
                    res.UV0 = PackedFloatVectorConverter.UnpackFloats(res.CompressedMesh.UV, 2, 2 * 4, 0, res.VertexCount);
                    if (res.CompressedMesh.UV.NumItems >= res.VertexCount * 4)
                    {
                        res.UV1 = PackedFloatVectorConverter.UnpackFloats(res.CompressedMesh.UV, 2, 2 * 4, res.VertexCount * 2, res.VertexCount);
                    }
                }
            }
            //BindPose
            if (version.LessThan(5))
            {
                if (res.CompressedMesh.BindPoses.NumItems > 0)
                {
                    res.BindPose = new Matrix4x4[res.CompressedMesh.BindPoses.NumItems / 16];
                    var m_BindPoses_Unpacked = PackedFloatVectorConverter.UnpackFloats(res.CompressedMesh.BindPoses, 16, 4 * 16);
                    var buffer = new float[16];
                    for (int i = 0; i < res.BindPose.Length; i++)
                    {
                        Array.Copy(m_BindPoses_Unpacked, i * 16, buffer, 0, 16);
                        res.BindPose[i] = UnityReaderExtension.CreateMatrix(buffer);
                    }
                }
            }
            //Normal
            if (res.CompressedMesh.Normals.NumItems > 0)
            {
                var normalData = PackedFloatVectorConverter.UnpackFloats(res.CompressedMesh.Normals, 2, 4 * 2);
                var signs = PackedIntVectorConverter.UnpackInts(res.CompressedMesh.NormalSigns);
                res.Normals = new float[res.CompressedMesh.Normals.NumItems / 2 * 3];
                for (int i = 0; i < res.CompressedMesh.Normals.NumItems / 2; ++i)
                {
                    var x = normalData[i * 2 + 0];
                    var y = normalData[i * 2 + 1];
                    var zsqr = 1 - x * x - y * y;
                    float z;
                    if (zsqr >= 0f)
                        z = (float)Math.Sqrt(zsqr);
                    else
                    {
                        z = 0;
                        var normal = new Vector3(x, y, z);
                        normal = Vector3.Normalize(normal);
                        x = normal.X;
                        y = normal.Y;
                        z = normal.Z;
                    }
                    if (signs[i] == 0)
                        z = -z;
                    res.Normals[i * 3] = x;
                    res.Normals[i * 3 + 1] = y;
                    res.Normals[i * 3 + 2] = z;
                }
            }
            //Tangent
            if (res.CompressedMesh.Tangents.NumItems > 0)
            {
                var tangentData = PackedFloatVectorConverter.UnpackFloats(res.CompressedMesh.Tangents, 2, 4 * 2);
                var signs = PackedIntVectorConverter.UnpackInts(res.CompressedMesh.TangentSigns);
                res.Tangents = new float[res.CompressedMesh.Tangents.NumItems / 2 * 4];
                for (int i = 0; i < res.CompressedMesh.Tangents.NumItems / 2; ++i)
                {
                    var x = tangentData[i * 2 + 0];
                    var y = tangentData[i * 2 + 1];
                    var zsqr = 1 - x * x - y * y;
                    float z;
                    if (zsqr >= 0f)
                        z = (float)Math.Sqrt(zsqr);
                    else
                    {
                        z = 0;
                        var vector3f = new Vector3(x, y, z);
                        vector3f = Vector3.Normalize(vector3f);
                        x = vector3f.X;
                        y = vector3f.Y;
                        z = vector3f.Z;
                    }
                    if (signs[i * 2 + 0] == 0)
                        z = -z;
                    var w = signs[i * 2 + 1] > 0 ? 1.0f : -1.0f;
                    res.Tangents[i * 4] = x;
                    res.Tangents[i * 4 + 1] = y;
                    res.Tangents[i * 4 + 2] = z;
                    res.Tangents[i * 4 + 3] = w;
                }
            }
            //FloatColor
            if (version.GreaterThanOrEquals(5))
            {
                if (res.CompressedMesh.FloatColors.NumItems > 0)
                {
                    res.Colors = PackedFloatVectorConverter.UnpackFloats(res.CompressedMesh.FloatColors, 1, 4);
                }
            }
            //Skin
            if (res.CompressedMesh.Weights.NumItems > 0)
            {
                var weights = PackedIntVectorConverter.UnpackInts(res.CompressedMesh.Weights);
                var boneIndices = PackedIntVectorConverter.UnpackInts(res.CompressedMesh.BoneIndices);

                InitMSkin(res);

                int bonePos = 0;
                int boneIndexPos = 0;
                int j = 0;
                int sum = 0;

                for (int i = 0; i < res.CompressedMesh.Weights.NumItems; i++)
                {
                    //read bone index and weight.
                    res.Skin[bonePos].Weight[j] = weights[i] / 31.0f;
                    res.Skin[bonePos].BoneIndex[j] = boneIndices[boneIndexPos++];
                    j++;
                    sum += weights[i];

                    //the weights add up to one. fill the rest for this vertex with zero, and continue with next one.
                    if (sum >= 31)
                    {
                        for (; j < 4; j++)
                        {
                            res.Skin[bonePos].Weight[j] = 0;
                            res.Skin[bonePos].BoneIndex[j] = 0;
                        }
                        bonePos++;
                        j = 0;
                        sum = 0;
                    }
                    //we read three weights, but they don't add up to one. calculate the fourth one, and read
                    //missing bone index. continue with next vertex.
                    else if (j == 3)
                    {
                        res.Skin[bonePos].Weight[j] = (31 - sum) / 31.0f;
                        res.Skin[bonePos].BoneIndex[j] = boneIndices[boneIndexPos++];
                        bonePos++;
                        j = 0;
                        sum = 0;
                    }
                }
            }
            //IndexBuffer
            if (res.CompressedMesh.Triangles.NumItems > 0)
            {
                res.IndexBuffer = Array.ConvertAll(PackedIntVectorConverter.UnpackInts(res.CompressedMesh.Triangles), x => (uint)x);
            }
            //Color
            if (res.CompressedMesh.Colors.NumItems > 0)
            {
                res.CompressedMesh.Colors.NumItems *= 4;
                res.CompressedMesh.Colors.BitSize /= 4;
                var tempColors = PackedIntVectorConverter.UnpackInts(res.CompressedMesh.Colors);
                res.Colors = new float[res.CompressedMesh.Colors.NumItems];
                for (int v = 0; v < res.CompressedMesh.Colors.NumItems; v++)
                {
                    res.Colors[v] = tempColors[v] / 255f;
                }
            }
        }

        private static void GetTriangles(Mesh res, Version version)
        {
            var items = new List<uint>();
            foreach (var m_SubMesh in res.SubMeshes)
            {
                var firstIndex = m_SubMesh.FirstByte / 2;
                if (!res.Use16BitIndices)
                {
                    firstIndex /= 2;
                }
                var indexCount = m_SubMesh.IndexCount;
                var topology = m_SubMesh.Topology;
                if (topology == GfxPrimitiveType.Triangles)
                {
                    for (int i = 0; i < indexCount; i += 3)
                    {
                        items.Add(res.IndexBuffer[firstIndex + i]);
                        items.Add(res.IndexBuffer[firstIndex + i + 1]);
                        items.Add(res.IndexBuffer[firstIndex + i + 2]);
                    }
                }
                else if (version.LessThan(4) || topology == GfxPrimitiveType.TriangleStrip)
                {
                    // de-stripify :
                    uint triIndex = 0;
                    for (int i = 0; i < indexCount - 2; i++)
                    {
                        var a = res.IndexBuffer[firstIndex + i];
                        var b = res.IndexBuffer[firstIndex + i + 1];
                        var c = res.IndexBuffer[firstIndex + i + 2];

                        // skip degenerates
                        if (a == b || a == c || b == c)
                            continue;

                        // do the winding flip-flop of strips :
                        if ((i & 1) == 1)
                        {
                            items.Add(b);
                            items.Add(a);
                        }
                        else
                        {
                            items.Add(a);
                            items.Add(b);
                        }
                        items.Add(c);
                        triIndex += 3;
                    }
                    //fix indexCount
                    m_SubMesh.IndexCount = triIndex;
                }
                else if (topology == GfxPrimitiveType.Quads)
                {
                    for (int q = 0; q < indexCount; q += 4)
                    {
                        items.Add(res.IndexBuffer[firstIndex + q]);
                        items.Add(res.IndexBuffer[firstIndex + q + 1]);
                        items.Add(res.IndexBuffer[firstIndex + q + 2]);
                        items.Add(res.IndexBuffer[firstIndex + q]);
                        items.Add(res.IndexBuffer[firstIndex + q + 2]);
                        items.Add(res.IndexBuffer[firstIndex + q + 3]);
                    }
                    //fix indexCount
                    m_SubMesh.IndexCount = indexCount / 2 * 3;
                }
                else
                {
                    throw new NotSupportedException("Failed getting triangles. Submesh topology is lines or points.");
                }
            }
            res.Indices = [..items];
        }

        private static void InitMSkin(Mesh res)
        {
            res.Skin = new BoneWeights4[res.VertexCount];
            for (int i = 0; i < res.VertexCount; i++)
            {
                res.Skin[i] = new BoneWeights4()
                {
                    Weight = new float[4],
                    BoneIndex = new int[4]
                };
            }
        }

        private static void SetUV(Mesh res, int uv, float[] m_UV)
        {
            switch (uv)
            {
                case 0:
                    res.UV0 = m_UV;
                    break;
                case 1:
                    res.UV1 = m_UV;
                    break;
                case 2:
                    res.UV2 = m_UV;
                    break;
                case 3:
                    res.UV3 = m_UV;
                    break;
                case 4:
                    res.UV4 = m_UV;
                    break;
                case 5:
                    res.UV5 = m_UV;
                    break;
                case 6:
                    res.UV6 = m_UV;
                    break;
                case 7:
                    res.UV7 = m_UV;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static float[] GetUV(Mesh res, int uv)
        {
            return uv switch
            {
                0 => res.UV0,
                1 => res.UV1,
                2 => res.UV2,
                3 => res.UV3,
                4 => res.UV4,
                5 => res.UV5,
                6 => res.UV6,
                7 => res.UV7,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    
}
