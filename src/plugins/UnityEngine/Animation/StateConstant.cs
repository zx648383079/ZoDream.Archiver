using System.Collections.Generic;

namespace UnityEngine
{
    public class StateConstant
    {
        public TransitionConstant[] TransitionConstantArray { get; set; }
        public int[] BlendTreeConstantIndexArray { get; set; }
        public LeafInfoConstant[] LeafInfoArray { get; set; }
        public BlendTreeConstant[] BlendTreeConstantArray { get; set; }
        public uint NameID { get; set; }
        public uint PathID { get; set; }
        public uint FullPathID { get; set; }
        public uint TagID { get; set; }
        public uint SpeedParamID { get; set; }
        public uint MirrorParamID { get; set; }
        public uint CycleOffsetParamID { get; set; }
        public float Speed { get; set; }
        public float CycleOffset { get; set; }
        public bool IKOnFeet { get; set; }
        public bool WriteDefaultValues { get; set; }
        public bool Loop { get; set; }
        public bool Mirror { get; set; }

    }
}
