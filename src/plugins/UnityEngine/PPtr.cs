using System;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine
{
    public interface IPPtr
    {
        public int FileID { get; }

        public long PathID { get; }
    }

    public struct PPtr : IPPtr, IEquatable<IPPtr>
    {
        public int FileID { get; set;}

        public long PathID { get; set; }

        public readonly bool Equals(IPPtr? other)
        {
            return other?.FileID == FileID && other.PathID == PathID;
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is IPPtr ptr)
            {
                return Equals(ptr);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FileID, PathID);
        }

        public override string ToString()
        {
            return $"[PPtr]{FileID}:{PathID}";
        }
        public static bool operator ==(PPtr left, PPtr right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PPtr left, PPtr right)
        {
            return !(left == right);
        }
    }
    public interface IPPtr<T> : IPPtr
        where T : Object
    {
        public bool IsNotNull { get; }
        [MemberNotNullWhen(true, nameof(IsNotNull))]
        public IResourceEntry? Resource { get; }
        /// <summary>
        /// 获取在 Resource 中的序号
        /// </summary>
        public int Index { get; }

        public bool TryGet([NotNullWhen(true)] out T? instance);
        public bool TryGet<K>([NotNullWhen(true)] out K? instance);
        /// <summary>
        /// 主要是确定宿主
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public IPPtr<K> Create<K>(IPPtr ptr) where K : Object;
    }
}
