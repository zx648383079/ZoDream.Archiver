namespace UnityEngine
{
    public class PPtr
    {

        public int FileID { get; set; }

        public long PathID { get; set; }
    }
    public sealed class PPtr<T> : PPtr
        where T : Object
    {
    }
}
