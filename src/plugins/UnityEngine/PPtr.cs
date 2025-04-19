namespace UnityEngine
{
    public sealed class PPtr<T> where T : Object
    {

        public int FileID { get; set; }

        public long PathID { get; set; }
    }
}
