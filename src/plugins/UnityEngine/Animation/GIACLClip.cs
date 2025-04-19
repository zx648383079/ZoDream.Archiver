namespace UnityEngine
{
    public class GIACLClip : ACLClip
    {
        public uint CurveCount { get; set; }
        public uint ConstCurveCount { get; set; }

        public byte[] ClipData { get; set; }
        public byte[] DatabaseData { get; set; }

        public override bool IsSet => ClipData is not null && ClipData.Length > 0 && DatabaseData is not null && DatabaseData.Length > 0;

    }

}
