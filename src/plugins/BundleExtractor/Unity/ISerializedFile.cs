﻿using System;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Unity
{
    internal interface ISerializedFile : IDisposable
    {
        public string FullPath { get; }

        public ILogger? Logger { get; }
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
        public IBundleBinaryReader Create(ObjectInfo info);

        public UIObject? this[long pathID] { get; }
        public void AddChild(UIObject obj);
    }
}
