namespace UnityEngine
{
    public class FloatCurve
    {
        public AnimationCurve<float> Curve { get; set; }
        public string Attribute { get; set; }
        public string Path { get; set; }
        public NativeClassID ClassID { get; set; }
        public IPPtr<MonoScript> Script { get; set; }
        public int Flags;

    }

}
