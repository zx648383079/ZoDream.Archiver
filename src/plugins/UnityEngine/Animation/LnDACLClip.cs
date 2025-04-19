namespace UnityEngine
{
    public class LnDACLClip : ACLClip
    {
        public uint CurveCount { get; set; }
        public byte[] ClipData { get; set; }

        public override bool IsSet => ClipData is not null && ClipData.Length > 0;
        
    }

}
