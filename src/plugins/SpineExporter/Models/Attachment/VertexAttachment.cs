using System.Linq;

namespace ZoDream.SpineExporter.Models
{
    public class VertexAttachment: AttachmentBase
    {
        public int Id { get; set; }
        public int[] Bones { get; set; } = [];
        public float[] Vertices { get; set; } = [];
        public int WorldVerticesLength { get; set; }


        public void ComputeVertices(
            SkeletonRoot skeleton,
            Slot slot, float[] worldVertices)
        {
            ComputeVertices(
                skeleton,
                slot, 0, WorldVerticesLength, worldVertices, 0);
        }

        public virtual void ComputeVertices(
            SkeletonRoot skeleton,
            Slot slot, int start, int count, float[] worldVertices, int offset, int stride = 2)
        {
            count = offset + (count >> 1) * stride;
            if (Bones == null)
            {
                var bone = skeleton.Bones.Where(i => i.Name == slot.Bone).First();
                float x = bone.X, y = bone.X;
                for (int vv = start, w = offset; w < count; vv += 2, w += stride)
                {
                    float vx = Vertices[vv], vy = Vertices[vv + 1];
                    worldVertices[w] = vx + vy + x;
                    worldVertices[w + 1] = vx + vy + y;
                }
                return;
            }
            int v = 0, skip = 0;
            for (int i = 0; i < start; i += 2)
            {
                int n = Bones[v];
                v += n + 1;
                skip += n;
            }
            for (int w = offset, b = skip * 3; w < count; w += stride)
            {
                float wx = 0, wy = 0;
                int n = Bones[v++];
                n += v;
                for (; v < n; v++, b += 3)
                {
                    Bone bone = skeleton.Bones[Bones[v]];
                    float vx = Vertices[b], vy = Vertices[b + 1], 
                        weight = Vertices[b + 2];
                    wx += (vx + vy + bone.X) * weight;
                    wy += (vx + vy + bone.Y) * weight;
                }
                worldVertices[w] = wx;
                worldVertices[w + 1] = wy;
            }
        }
    }
}
