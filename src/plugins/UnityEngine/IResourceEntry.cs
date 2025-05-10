using System.Diagnostics.CodeAnalysis;

namespace UnityEngine
{
    public interface IResourceEntry
    {
        public int Count { get; }
        public Object? this[int index] { get; }

        public int IndexOf(long pathID);
        public int IndexOf(Object obj);
        public bool TryGet<T>(PPtr ptr, [NotNullWhen(true)] out T? instance);
    }
}
