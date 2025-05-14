using Mono.Cecil;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class SpineExporter : IMultipartExporter
    {
        public SpineExporter(IPPtr<GameObject> ptr)
            : this(ptr.Index, (ISerializedFile)ptr.Resource)
        {
        }
        public SpineExporter(int entryId, ISerializedFile resource)
        {
            _entryId = entryId;
            _resource = resource;
            _assembly = _resource.Container!.Assembly;
            _converter = new(resource);
            var obj = _resource[_entryId] as GameObject;
            Debug.Assert(obj is not null);
            FileName = obj.Name ?? string.Empty;
            Initialize(obj);
        }

  

        private readonly DocumentReader _converter;
        private readonly IAssemblyReader _assembly;
        private readonly int _entryId;
        private readonly ISerializedFile _resource;

        public string FileName { get; private set; }
        public string SourcePath => _resource.FullPath;

        public bool IsEmpty => false;


        private void Initialize(GameObject game)
        {
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (_resource[_entryId] is not TextAsset asset)
            {
                return;
            }
            SaveAs(asset, fileName, mode);
        }
        public static void SaveAs(TextAsset asset, string fileName, ArchiveExtractMode mode)
        {
            if (fileName.EndsWith(".skel"))
            {
                fileName = fileName[..^5];
            }
            if (!LocationStorage.TryCreate(fileName, ".skel.json", mode, out fileName))
            {
                return;
            }
            asset.Script.Position = 0;
            asset.Script.SaveAs(fileName);
        }

        public static bool IsSupport(byte[] buffer, int count)
        {
            if (count < 10)
            {
                return false;
            }
            return buffer[0] == '{' && buffer.IndexOf(Encoding.ASCII.GetBytes("\"skeleton\"")) > 0;
        }

        public void Dispose()
        {
        }
    }
}
