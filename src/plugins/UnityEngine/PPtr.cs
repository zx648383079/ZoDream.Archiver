using System.Diagnostics.CodeAnalysis;

namespace UnityEngine
{
    public struct PPtr
    {
        public int FileID;

        public long PathID;
    }
    public interface IPPtr<T>
        where T : Object
    {
        public PPtr Value { get; }

        public int FileID { get; }

        public long PathID { get; }

        public bool IsNull { get; }

        public bool TryGet([NotNullWhen(true)] out T? instance);
        public bool TryGet<K>([NotNullWhen(true)] out K? instance);
    }
}
