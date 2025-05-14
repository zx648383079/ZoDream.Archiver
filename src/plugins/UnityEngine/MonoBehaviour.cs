namespace UnityEngine
{
    public class MonoBehaviour : Behaviour
    {
        public IPPtr<MonoScript> Script;

        public long DataOffset {  get; set; }
    }
}
