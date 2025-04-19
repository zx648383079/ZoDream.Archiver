using System.Collections.Generic;

namespace UnityEngine
{
    public class BlendShapeData
    {
        public List<BlendShapeVertex> Vertices;
        public List<MeshBlendShape> Shapes;
        public List<MeshBlendShapeChannel> Channels;
        public float[] FullWeights;
    }

}
