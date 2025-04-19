namespace UnityEngine
{
    public class BlendTreeNodeConstant
    {
        public uint BlendType { get; set; }
        public uint BlendEventID { get; set; }
        public uint BlendEventYID { get; set; }
        public uint[] ChildIndices { get; set; }
        public float[] ChildThresholdArray { get; set; }
        public Blend1dDataConstant Blend1dData { get; set; }
        public Blend2dDataConstant Blend2dData { get; set; }
        public BlendDirectDataConstant BlendDirectData { get; set; }
        public uint ClipID { get; set; }
        public uint ClipIndex { get; set; }
        public float Duration { get; set; }
        public float CycleOffset { get; set; }
        public bool Mirror { get; set; }

    }
}
