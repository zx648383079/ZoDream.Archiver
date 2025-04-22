using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using Object = UnityEngine.Object;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity
{
    internal interface ISerializedFile : IResourceEntry, IDisposable
    {
        public string FullPath { get; }

        public ILogger? Logger { get; }
        public IBundleContainer? Container { get; }

        public FormatVersion Format { get; }

        public Version Version { get; }
        public BuildTarget Platform { get; }
        public IEnumerable<string> Dependencies { get; }
        public SerializedType[] TypeItems { get; }
        public string GetDependency(int index);
        /// <summary>
        /// 根据依赖名 获取依赖的位置
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public int IndexOf(string dependency);

        public int AddDependency(string dependency);

        public new Object? this[int index] { get; set; }
        public IBundleBinaryReader OpenRead(int index);
        public IBundleBinaryReader OpenRead(ObjectInfo info);
        /// <summary>
        /// 获取原始结构信息
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ObjectInfo Get(int index);
        public Stream OpenResource(ResourceSource source);
    }
}
