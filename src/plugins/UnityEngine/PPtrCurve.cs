namespace UnityEngine
{
    public class PPtrCurve
    {
        public PPtrKeyframe[] Curve { get; set; }
        public string Attribute { get; set; }
        public string Path { get; set; }
        public int ClassID { get; set; }
        public PPtr<MonoScript> Script { get; set; }
        public int Flags { get; set; }
    }
}
