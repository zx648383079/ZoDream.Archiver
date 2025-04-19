namespace UnityEngine
{
    public class TransitionConstant
    {
        public ConditionConstant[] ConditionConstantArray { get; set; }
        public uint DestinationState { get; set; }
        public uint FullPathID { get; set; }
        public uint ID { get; set; }
        public uint UserID { get; set; }
        public float TransitionDuration { get; set; }
        public float TransitionOffset { get; set; }
        public float ExitTime { get; set; }
        public bool HasExitTime { get; set; }
        public bool HasFixedDuration { get; set; }
        public int InterruptionSource { get; set; }
        public bool OrderedInterruption { get; set; }
        public bool Atomic { get; set; }
        public bool CanTransitionToSelf { get; set; }

    }
}
