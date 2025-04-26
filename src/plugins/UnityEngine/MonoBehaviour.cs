
namespace UnityEngine
{
    public sealed class MonoBehaviour : Behaviour
    {
        public IPPtr<MonoScript> Script;

        public long DataOffset {  get; set; }
    }
}
