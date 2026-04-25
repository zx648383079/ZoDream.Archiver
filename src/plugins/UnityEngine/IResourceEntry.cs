using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine.Document;
using ZoDream.Shared.Bundle;

namespace UnityEngine
{
    public interface IResourceEntry
    {
        public int Count { get; }

        public Version Version { get; }

        public int IndexOf(long pathID);
        public int IndexOf(Object obj);

        public bool TryGet(int index, [NotNullWhen(true)] out Object? instance);

        /// <summary>
        /// 根据序号获取解析流
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IBundleBinaryReader OpenRead(int index);
        /// <summary>
        /// 获取 type tree 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VirtualDocument? GetType(int index);
        public Stream OpenResource(ResourceSource source);
    }
}
