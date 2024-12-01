using System;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BlendShapeData: IElementLoader
    {
        public List<BlendShapeVertex> vertices;
        public List<MeshBlendShape> shapes;
        public List<MeshBlendShapeChannel> channels;
        public float[] fullWeights;
        public static bool HasVarintVertices(SerializedType type) => Convert.ToHexString(type.OldTypeHash) == "70AE601CDF0C273E745D9EC1333426A4";

        public void ReadBase(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            var scanner = reader.Get<IBundleElementScanner>();

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                int numVerts = reader.ReadInt32();
                vertices = [];
                for (int i = 0; i < numVerts; i++)
                {
                    vertices.Add(new BlendShapeVertex(reader));
                }

                int numShapes = reader.ReadInt32();
                shapes = [];
                for (int i = 0; i < numShapes; i++)
                {
                    var shape = new MeshBlendShape();
                    scanner.TryRead(reader, shape);
                    shapes.Add(shape);
                }

                
            }
            else
            {
                var m_ShapesSize = reader.ReadInt32();
                var m_Shapes = new List<MeshBlendShape>();
                for (int i = 0; i < m_ShapesSize; i++)
                {
                    var shape = new MeshBlendShape();
                    scanner.TryRead(reader, shape);
                    m_Shapes.Add(shape);
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

        public void Read(IBundleBinaryReader reader)
        {
            ReadBase(reader);
            var version = reader.Get<UnityVersion>();

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {

                int numChannels = reader.ReadInt32();
                channels = [];
                for (int i = 0; i < numChannels; i++)
                {
                    channels.Add(new MeshBlendShapeChannel(reader));
                }

                fullWeights = reader.ReadArray(r => r.ReadSingle());

            }
        }
    }

}
