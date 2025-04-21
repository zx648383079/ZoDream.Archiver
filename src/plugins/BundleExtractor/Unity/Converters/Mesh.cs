using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    
    internal class MeshConverter : BundleConverter<Mesh>, IBundleExporter
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
                    if (version.GreaterThanOrEquals(2017, 3, 1, VersionType.Patch, 1) && m_MeshCompression == 0)//2017.3.xfx with no compression
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

        public void ReadBase(Mesh res, IBundleBinaryReader reader, IBundleSerializer serializer, Action cb)
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

            res.SubMeshes = reader.ReadArray(_ => serializer.Deserialize<SubMesh>(reader));

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
            }

            ProcessData();
        }

        private void ProcessData()
        {
            var version = Reader.Version;
            if (!string.IsNullOrEmpty(m_StreamData?.path))
            {
                if (m_VertexData.m_VertexCount > 0)
                {
                    m_VertexData.m_DataSize = Reader.OpenResource(m_StreamData).ToArray();
                }
            }
            if (version.GreaterThanOrEquals(3, 5)) //3.5 and up
            {
                ReadVertexData();
            }

            if (m_CollisionMeshBaked)
            {
                return;
            }

            if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and later
            {
                DecompressCompressedMesh();
            }

            GetTriangles();
        }

        private void ReadVertexData()
        {
            m_VertexCount = (int)m_VertexData.m_VertexCount;

            for (var chn = 0; chn < m_VertexData.m_Channels.Count; chn++)
            {
                var m_Channel = m_VertexData.m_Channels[chn];
                if (m_Channel.dimension > 0)
                {
                    var m_Stream = m_VertexData.m_Streams[m_Channel.stream];
                    var channelMask = new BitArray([(int)m_Stream.channelMask]);
                    if (channelMask.Get(chn))
                    {
                        if (Reader.Version.LessThan(2018) && chn == 2 && m_Channel.format == 2) //kShaderChannelColor && kChannelFormatColor
                        {
                            m_Channel.dimension = 4;
                        }

                        var vertexFormat = MeshHelper.ToVertexFormat(m_Channel.format, Reader.Version);
                        var componentByteSize = (int)MeshHelper.GetFormatSize(vertexFormat);
                        var componentBytes = new byte[m_VertexCount * m_Channel.dimension * componentByteSize];
                        for (int v = 0; v < m_VertexCount; v++)
                        {
                            var vertexOffset = (int)m_Stream.offset + m_Channel.offset + (int)m_Stream.stride * v;
                            for (int d = 0; d < m_Channel.dimension; d++)
                            {
                                var componentOffset = vertexOffset + componentByteSize * d;
                                Buffer.BlockCopy(m_VertexData.m_DataSize, componentOffset, componentBytes, componentByteSize * (v * m_Channel.dimension + d), componentByteSize);
                            }
                        }

                        if (Reader.EndianType == EndianType.BigEndian && componentByteSize > 1) //swap bytes
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

                        if (Reader.Version.GreaterThanOrEquals(2018))
                        {
                            switch (chn)
                            {
                                case 0: //kShaderChannelVertex
                                    m_Vertices = componentsFloatArray;
                                    break;
                                case 1: //kShaderChannelNormal
                                    m_Normals = componentsFloatArray;
                                    break;
                                case 2: //kShaderChannelTangent
                                    m_Tangents = componentsFloatArray;
                                    break;
                                case 3: //kShaderChannelColor
                                    m_Colors = componentsFloatArray;
                                    break;
                                case 4: //kShaderChannelTexCoord0
                                    m_UV0 = componentsFloatArray;
                                    break;
                                case 5: //kShaderChannelTexCoord1
                                    m_UV1 = componentsFloatArray;
                                    break;
                                case 6: //kShaderChannelTexCoord2
                                    m_UV2 = componentsFloatArray;
                                    break;
                                case 7: //kShaderChannelTexCoord3
                                    m_UV3 = componentsFloatArray;
                                    break;
                                case 8: //kShaderChannelTexCoord4
                                    m_UV4 = componentsFloatArray;
                                    break;
                                case 9: //kShaderChannelTexCoord5
                                    m_UV5 = componentsFloatArray;
                                    break;
                                case 10: //kShaderChannelTexCoord6
                                    m_UV6 = componentsFloatArray;
                                    break;
                                case 11: //kShaderChannelTexCoord7
                                    m_UV7 = componentsFloatArray;
                                    break;
                                //2018.2 and up
                                case 12: //kShaderChannelBlendWeight
                                    if (m_Skin == null)
                                    {
                                        InitMSkin();
                                    }
                                    for (int i = 0; i < m_VertexCount; i++)
                                    {
                                        for (int j = 0; j < m_Channel.dimension; j++)
                                        {
                                            m_Skin[i].weight[j] = componentsFloatArray[i * m_Channel.dimension + j];
                                        }
                                    }
                                    break;
                                case 13: //kShaderChannelBlendIndices
                                    if (m_Skin == null)
                                    {
                                        InitMSkin();
                                    }
                                    for (int i = 0; i < m_VertexCount; i++)
                                    {
                                        for (int j = 0; j < m_Channel.dimension; j++)
                                        {
                                            m_Skin[i].boneIndex[j] = componentsIntArray[i * m_Channel.dimension + j];
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
                                    m_Vertices = componentsFloatArray;
                                    break;
                                case 1: //kShaderChannelNormal
                                    m_Normals = componentsFloatArray;
                                    break;
                                case 2: //kShaderChannelColor
                                    m_Colors = componentsFloatArray;
                                    break;
                                case 3: //kShaderChannelTexCoord0
                                    m_UV0 = componentsFloatArray;
                                    break;
                                case 4: //kShaderChannelTexCoord1
                                    m_UV1 = componentsFloatArray;
                                    break;
                                case 5:
                                    if (Reader.Version.Major >= 5) //kShaderChannelTexCoord2
                                    {
                                        m_UV2 = componentsFloatArray;
                                    }
                                    else //kShaderChannelTangent
                                    {
                                        m_Tangents = componentsFloatArray;
                                    }
                                    break;
                                case 6: //kShaderChannelTexCoord3
                                    m_UV3 = componentsFloatArray;
                                    break;
                                case 7: //kShaderChannelTangent
                                    m_Tangents = componentsFloatArray;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void DecompressCompressedMesh()
        {
            //Vertex
            if (m_CompressedMesh.m_Vertices.m_NumItems > 0)
            {
                m_VertexCount = (int)m_CompressedMesh.m_Vertices.m_NumItems / 3;
                m_Vertices = m_CompressedMesh.m_Vertices.UnpackFloats(3, 3 * 4);
            }
            //UV
            if (m_CompressedMesh.m_UV.m_NumItems > 0)
            {
                var m_UVInfo = m_CompressedMesh.m_UVInfo;
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
                            var m_UV = m_CompressedMesh.m_UV.UnpackFloats(uvDim, uvDim * 4, uvSrcOffset, m_VertexCount);
                            SetUV(uv, m_UV);
                            uvSrcOffset += uvDim * m_VertexCount;
                        }
                    }
                }
                else
                {
                    m_UV0 = m_CompressedMesh.m_UV.UnpackFloats(2, 2 * 4, 0, m_VertexCount);
                    if (m_CompressedMesh.m_UV.m_NumItems >= m_VertexCount * 4)
                    {
                        m_UV1 = m_CompressedMesh.m_UV.UnpackFloats(2, 2 * 4, m_VertexCount * 2, m_VertexCount);
                    }
                }
            }
            //BindPose
            if (Reader.Version.LessThan(5))
            {
                if (m_CompressedMesh.m_BindPoses.m_NumItems > 0)
                {
                    m_BindPose = new Matrix4x4[m_CompressedMesh.m_BindPoses.m_NumItems / 16];
                    var m_BindPoses_Unpacked = m_CompressedMesh.m_BindPoses.UnpackFloats(16, 4 * 16);
                    var buffer = new float[16];
                    for (int i = 0; i < m_BindPose.Length; i++)
                    {
                        Array.Copy(m_BindPoses_Unpacked, i * 16, buffer, 0, 16);
                        m_BindPose[i] = UnityReaderExtension.CreateMatrix(buffer);
                    }
                }
            }
            //Normal
            if (m_CompressedMesh.m_Normals.m_NumItems > 0)
            {
                var normalData = m_CompressedMesh.m_Normals.UnpackFloats(2, 4 * 2);
                var signs = m_CompressedMesh.m_NormalSigns.UnpackInts();
                m_Normals = new float[m_CompressedMesh.m_Normals.m_NumItems / 2 * 3];
                for (int i = 0; i < m_CompressedMesh.m_Normals.m_NumItems / 2; ++i)
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
                    m_Normals[i * 3] = x;
                    m_Normals[i * 3 + 1] = y;
                    m_Normals[i * 3 + 2] = z;
                }
            }
            //Tangent
            if (m_CompressedMesh.m_Tangents.m_NumItems > 0)
            {
                var tangentData = m_CompressedMesh.m_Tangents.UnpackFloats(2, 4 * 2);
                var signs = m_CompressedMesh.m_TangentSigns.UnpackInts();
                m_Tangents = new float[m_CompressedMesh.m_Tangents.m_NumItems / 2 * 4];
                for (int i = 0; i < m_CompressedMesh.m_Tangents.m_NumItems / 2; ++i)
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
                    m_Tangents[i * 4] = x;
                    m_Tangents[i * 4 + 1] = y;
                    m_Tangents[i * 4 + 2] = z;
                    m_Tangents[i * 4 + 3] = w;
                }
            }
            //FloatColor
            if (Reader.Version.GreaterThanOrEquals(5))
            {
                if (m_CompressedMesh.m_FloatColors.m_NumItems > 0)
                {
                    m_Colors = m_CompressedMesh.m_FloatColors.UnpackFloats(1, 4);
                }
            }
            //Skin
            if (m_CompressedMesh.m_Weights.m_NumItems > 0)
            {
                var weights = m_CompressedMesh.m_Weights.UnpackInts();
                var boneIndices = m_CompressedMesh.m_BoneIndices.UnpackInts();

                InitMSkin();

                int bonePos = 0;
                int boneIndexPos = 0;
                int j = 0;
                int sum = 0;

                for (int i = 0; i < m_CompressedMesh.m_Weights.m_NumItems; i++)
                {
                    //read bone index and weight.
                    m_Skin[bonePos].weight[j] = weights[i] / 31.0f;
                    m_Skin[bonePos].boneIndex[j] = boneIndices[boneIndexPos++];
                    j++;
                    sum += weights[i];

                    //the weights add up to one. fill the rest for this vertex with zero, and continue with next one.
                    if (sum >= 31)
                    {
                        for (; j < 4; j++)
                        {
                            m_Skin[bonePos].weight[j] = 0;
                            m_Skin[bonePos].boneIndex[j] = 0;
                        }
                        bonePos++;
                        j = 0;
                        sum = 0;
                    }
                    //we read three weights, but they don't add up to one. calculate the fourth one, and read
                    //missing bone index. continue with next vertex.
                    else if (j == 3)
                    {
                        m_Skin[bonePos].weight[j] = (31 - sum) / 31.0f;
                        m_Skin[bonePos].boneIndex[j] = boneIndices[boneIndexPos++];
                        bonePos++;
                        j = 0;
                        sum = 0;
                    }
                }
            }
            //IndexBuffer
            if (m_CompressedMesh.m_Triangles.m_NumItems > 0)
            {
                m_IndexBuffer = Array.ConvertAll(m_CompressedMesh.m_Triangles.UnpackInts(), x => (uint)x);
            }
            //Color
            if (m_CompressedMesh.m_Colors?.m_NumItems > 0)
            {
                m_CompressedMesh.m_Colors.m_NumItems *= 4;
                m_CompressedMesh.m_Colors.m_BitSize /= 4;
                var tempColors = m_CompressedMesh.m_Colors.UnpackInts();
                m_Colors = new float[m_CompressedMesh.m_Colors.m_NumItems];
                for (int v = 0; v < m_CompressedMesh.m_Colors.m_NumItems; v++)
                {
                    m_Colors[v] = tempColors[v] / 255f;
                }
            }
        }

        private void GetTriangles()
        {
            foreach (var m_SubMesh in m_SubMeshes)
            {
                var firstIndex = m_SubMesh.firstByte / 2;
                if (!m_Use16BitIndices)
                {
                    firstIndex /= 2;
                }
                var indexCount = m_SubMesh.indexCount;
                var topology = m_SubMesh.topology;
                if (topology == GfxPrimitiveType.Triangles)
                {
                    for (int i = 0; i < indexCount; i += 3)
                    {
                        m_Indices.Add(m_IndexBuffer[firstIndex + i]);
                        m_Indices.Add(m_IndexBuffer[firstIndex + i + 1]);
                        m_Indices.Add(m_IndexBuffer[firstIndex + i + 2]);
                    }
                }
                else if (Reader.Version.LessThan(4) || topology == GfxPrimitiveType.TriangleStrip)
                {
                    // de-stripify :
                    uint triIndex = 0;
                    for (int i = 0; i < indexCount - 2; i++)
                    {
                        var a = m_IndexBuffer[firstIndex + i];
                        var b = m_IndexBuffer[firstIndex + i + 1];
                        var c = m_IndexBuffer[firstIndex + i + 2];

                        // skip degenerates
                        if (a == b || a == c || b == c)
                            continue;

                        // do the winding flip-flop of strips :
                        if ((i & 1) == 1)
                        {
                            m_Indices.Add(b);
                            m_Indices.Add(a);
                        }
                        else
                        {
                            m_Indices.Add(a);
                            m_Indices.Add(b);
                        }
                        m_Indices.Add(c);
                        triIndex += 3;
                    }
                    //fix indexCount
                    m_SubMesh.indexCount = triIndex;
                }
                else if (topology == GfxPrimitiveType.Quads)
                {
                    for (int q = 0; q < indexCount; q += 4)
                    {
                        m_Indices.Add(m_IndexBuffer[firstIndex + q]);
                        m_Indices.Add(m_IndexBuffer[firstIndex + q + 1]);
                        m_Indices.Add(m_IndexBuffer[firstIndex + q + 2]);
                        m_Indices.Add(m_IndexBuffer[firstIndex + q]);
                        m_Indices.Add(m_IndexBuffer[firstIndex + q + 2]);
                        m_Indices.Add(m_IndexBuffer[firstIndex + q + 3]);
                    }
                    //fix indexCount
                    m_SubMesh.indexCount = indexCount / 2 * 3;
                }
                else
                {
                    throw new NotSupportedException("Failed getting triangles. Submesh topology is lines or points.");
                }
            }
        }

        private void InitMSkin()
        {
            m_Skin = [];
            for (int i = 0; i < m_VertexCount; i++)
            {
                m_Skin.Add(new BoneWeights4());
            }
        }

        private void SetUV(int uv, float[] m_UV)
        {
            switch (uv)
            {
                case 0:
                    m_UV0 = m_UV;
                    break;
                case 1:
                    m_UV1 = m_UV;
                    break;
                case 2:
                    m_UV2 = m_UV;
                    break;
                case 3:
                    m_UV3 = m_UV;
                    break;
                case 4:
                    m_UV4 = m_UV;
                    break;
                case 5:
                    m_UV5 = m_UV;
                    break;
                case 6:
                    m_UV6 = m_UV;
                    break;
                case 7:
                    m_UV7 = m_UV;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float[] GetUV(int uv)
        {
            return uv switch
            {
                0 => m_UV0,
                1 => m_UV1,
                2 => m_UV2,
                3 => m_UV3,
                4 => m_UV4,
                5 => m_UV5,
                6 => m_UV6,
                7 => m_UV7,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        public override void Associated(IDependencyBuilder? builder)
        {
            base.Associated(builder);
            if (!string.IsNullOrEmpty(m_StreamData?.path))
            {
                builder?.AddDependencyEntry(Reader.FullPath, FileID, m_StreamData.path);
            }
        }
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (m_VertexCount < 0 || m_Vertices == null || m_Vertices.Length == 0)
            {
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".obj", mode, out fileName))
            {
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine("g " + m_Name);
            #region Vertices
            int c = 3;
            if (m_Vertices.Length == m_VertexCount * 4)
            {
                c = 4;
            }
            for (int v = 0; v < m_VertexCount; v++)
            {
                sb.AppendFormat("v {0} {1} {2}\r\n", -m_Vertices[v * c], m_Vertices[v * c + 1], m_Vertices[v * c + 2]);
            }
            #endregion

            #region UV
            if (m_UV0?.Length > 0)
            {
                c = 4;
                if (m_UV0.Length == m_VertexCount * 2)
                {
                    c = 2;
                }
                else if (m_UV0.Length == m_VertexCount * 3)
                {
                    c = 3;
                }
                for (int v = 0; v < m_VertexCount; v++)
                {
                    sb.AppendFormat("vt {0} {1}\r\n", m_UV0[v * c], m_UV0[v * c + 1]);
                }
            }
            #endregion

            #region Normals
            if (m_Normals?.Length > 0)
            {
                if (m_Normals.Length == m_VertexCount * 3)
                {
                    c = 3;
                }
                else if (m_Normals.Length == m_VertexCount * 4)
                {
                    c = 4;
                }
                for (int v = 0; v < m_VertexCount; v++)
                {
                    sb.AppendFormat("vn {0} {1} {2}\r\n", -m_Normals[v * c], m_Normals[v * c + 1], m_Normals[v * c + 2]);
                }
            }
            #endregion

            #region Face
            int sum = 0;
            for (var i = 0; i < m_SubMeshes.Count; i++)
            {
                sb.AppendLine($"g {m_Name}_{i}");
                int indexCount = (int)m_SubMeshes[i].indexCount;
                var end = sum + indexCount / 3;
                for (int f = sum; f < end; f++)
                {
                    sb.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\r\n", m_Indices[f * 3 + 2] + 1, m_Indices[f * 3 + 1] + 1, m_Indices[f * 3] + 1);
                }
                sum = end;
            }
            #endregion

            sb.Replace("NaN", "0");
            File.WriteAllText(fileName, sb.ToString());
        }
    }

    
}
