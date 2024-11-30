using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Unity.SerializedFiles;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BlendShapeData
    {
        public List<BlendShapeVertex> vertices;
        public List<MeshBlendShape> shapes;
        public List<MeshBlendShapeChannel> channels;
        public float[] fullWeights;
        public static bool HasVarintVertices(SerializedType type) => Convert.ToHexString(type.OldTypeHash) == "70AE601CDF0C273E745D9EC1333426A4";

        public BlendShapeData(UIReader reader)
        {
            var version = reader.Version;

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                int numVerts = reader.ReadInt32();
                vertices = new List<BlendShapeVertex>();
                for (int i = 0; i < numVerts; i++)
                {
                    vertices.Add(new BlendShapeVertex(reader));
                }

                int numShapes = reader.ReadInt32();
                shapes = new List<MeshBlendShape>();
                for (int i = 0; i < numShapes; i++)
                {
                    shapes.Add(new MeshBlendShape(reader));
                }

                if (reader.IsLoveAndDeepSpace())
                {
                    reader.AlignStream();
                }

                int numChannels = reader.ReadInt32();
                channels = new List<MeshBlendShapeChannel>();
                for (int i = 0; i < numChannels; i++)
                {
                    channels.Add(new MeshBlendShapeChannel(reader));
                }

                fullWeights = reader.ReadArray(r => r.ReadSingle());
                if (reader.IsLoveAndDeepSpace())
                {
                    var varintVerticesSize = reader.ReadInt32();
                    if (varintVerticesSize > 0)
                    {
                        var pos = reader.Position;
                        while (reader.Position < pos + varintVerticesSize)
                        {
                            var value = reader.ReadUInt32();
                            var index = value & 0x0FFFFFFF;
                            var flags = value >> 0x1D;
                            var blendShapeVertex = new BlendShapeVertex
                            {
                                index = index,
                                vertex = (flags & 4) != 0 ? reader.ReadVector3() : Vector3.Zero,
                                normal = (flags & 2) != 0 ? reader.ReadVector3() : Vector3.Zero,
                                tangent = (flags & 1) != 0 ? reader.ReadVector3() : Vector3.Zero,
                            };
                            vertices.Add(blendShapeVertex);
                        }
                        reader.AlignStream();

                        var stride = (uint)(varintVerticesSize / vertices.Count);
                        foreach (var shape in shapes)
                        {
                            shape.firstVertex /= stride;
                        }
                    }
                }
                if (reader.IsShiningNikki() && version.GreaterThanOrEquals(2019))
                {
                    var varintVertices = reader.ReadArray(r => r.ReadByte());

                }
            }
            else
            {
                var m_ShapesSize = reader.ReadInt32();
                var m_Shapes = new List<MeshBlendShape>();
                for (int i = 0; i < m_ShapesSize; i++)
                {
                    m_Shapes.Add(new MeshBlendShape(reader));
                }
                reader.AlignStream();
                var m_ShapeVerticesSize = reader.ReadInt32();
                var m_ShapeVertices = new List<BlendShapeVertex>(); //MeshBlendShapeVertex
                for (int i = 0; i < m_ShapeVerticesSize; i++)
                {
                    m_ShapeVertices.Add(new BlendShapeVertex(reader));
                }
            }
        }
    }

}
