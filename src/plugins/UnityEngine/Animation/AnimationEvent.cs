namespace UnityEngine
{
    public class AnimationEvent
    {
        public float Time { get; set; }
        public string FunctionName { get; set; }
        public string Data { get; set; }
        public PPtr<Object> ObjectReferenceParameter { get; set; }
        public float FloatParameter { get; set; }
        public int IntParameter { get; set; }
        public int MessageOptions { get; set; }
    }
}
