using System;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity
{
    public interface ISerializedFile : IDisposable
    {
        public string FullPath { get; }
        public IBundleContainer? Container { get; }

        public FormatVersion Version { get; }

        public UnityVersion UnityVersion { get; }
        public List<UIObject> Children { get; }
        public BuildTarget Platform { get; }
        public IEnumerable<string> Dependencies { get; }
        public IEnumerable<ObjectInfo> ObjectMetaItems { get; }

        public SerializedType[] TypeItems { get; }
        public string GetDependency(int index);

        public int IndexOf(string dependency);

        public int AddDependency(string dependency);
        public EndianReader Create(ObjectInfo info);

        public UIObject? this[long pathID] { get; }
        public void AddChild(UIObject obj);
    }
}
