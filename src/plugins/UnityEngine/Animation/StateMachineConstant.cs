namespace UnityEngine
{
    public class StateMachineConstant
    {
        public StateConstant[] StateConstantArray { get; set; }
        public TransitionConstant[] AnyStateTransitionConstantArray { get; set; }
        public SelectorStateConstant[] SelectorStateConstantArray { get; set; }
        public uint DefaultState { get; set; }
        public uint MotionSetCount { get; set; }

    }

}
