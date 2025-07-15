using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.BundleExtractor.Unity.Spine;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using static ZoDream.BundleExtractor.Unity.Spine.SpineSkeletonDataAsset;

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
            Expectation.ThrowIfNot(obj is not null);
            FileName = obj.Name ?? string.Empty;
            Initialize(obj);
        }

        private readonly DocumentReader _converter;
        private readonly IAssemblyReader _assembly;
        private readonly int _entryId;
        private readonly ISerializedFile _resource;

        public string FileName { get; private set; }
        public IFilePath SourcePath => _resource.FullPath;
        public bool IsEmpty => _skeleton is null;

        private IPPtr<TextAsset>? _skeleton;
        private readonly HashSet<IPPtr<Shader>> _shaders = [];
        private readonly HashSet<IPPtr<Texture2D>> _textures = [];
        private readonly HashSet<IPPtr<TextAsset>> _atlases = [];

        private void Initialize(GameObject game)
        {
            foreach (var pptr in game.Components)
            {
                pptr.IsExclude = true;
                if (!pptr.TryGet(out var instance))
                {
                    continue;
                }
                if (instance is not MonoBehaviour behaviour)
                {
                    continue;
                }
                if (!behaviour.Script.TryGet(out var script))
                {
                    continue;
                }
                var data = BehaviorExporter.Deserialize<SpineSkeletonGraphic>(pptr.Create<MonoBehaviour>(pptr), _assembly, _converter);
                AddMaterial(data.AdditiveMaterial);
                AddMaterial(data.MultiplyMaterial);
                AddMaterial(data.Material);
                if (data.SkeletonDataAsset is null)
                {
                    continue;
                }
                data.SkeletonDataAsset.IsExclude = true;
                var asset = BehaviorExporter.Deserialize<SpineSkeletonDataAsset>(data.SkeletonDataAsset, _assembly, _converter);
                if (asset is null)
                {
                    continue;
                }
                if (asset.SkeletonJSON?.IsNotNull == true)
                {
                    asset.SkeletonJSON.IsExclude = true;
                    _skeleton = asset.SkeletonJSON;
                }
                AddMaterial(asset.BlendModeMaterials.MultiplyMaterials);
                AddMaterial(asset.BlendModeMaterials.AdditiveMaterials);
                AddMaterial(asset.BlendModeMaterials.ScreenMaterials);
                foreach (var ptr in asset.AtlasAssets)
                {
                    if (!ptr.IsNotNull)
                    {
                        continue;
                    }
                    ptr.IsExclude = true;
                    var item = BehaviorExporter.Deserialize<SpineAtlasAsset>(ptr, _assembly, _converter);
                    if (item is null)
                    {
                        continue;
                    }
                    AddMaterial(item.Materials);
                    item.AtlasFile.IsExclude = true;
                    if (item.AtlasFile?.IsNotNull == true)
                    {
                        _atlases.Add(item.AtlasFile);
                    }
                }
            }
        }
        private void AddMaterial(SpineReplacementMaterial[] data)
        {
            foreach (var item in data)
            {
                AddMaterial(item.Material);
            }
        }
        private void AddMaterial(IPPtr<Material>[] items)
        {
            foreach (var item in items)
            {
                AddMaterial(item);
            }
        }
        private void AddMaterial(IPPtr<Material> ptr)
        {
            if (ptr?.IsNotNull != true)
            {
                return;
            }
            if (!ptr.TryGet(out var material))
            {
                return;
            }
            material.Shader.IsExclude = true;
            if (material.Shader.IsNotNull == true)
            {
                _shaders.Add(material.Shader);
            }
            foreach (var item in material.SavedProperties.TexEnvs)
            {
                var pptr = item.Value.Texture;
                pptr.IsExclude = true;
                if (ptr.IsValid)
                {
                    _textures.Add(pptr.Create<Texture2D>(pptr));
                }
            }
        }
  
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (IsEmpty)
            {
                return;
            }
            var folder = fileName;
            var baseFileName = Path.Combine(folder, FileName);
            if (_skeleton.TryGet(out var asset))
            {
                SaveAs(asset, baseFileName, mode);
            }
            foreach (var ptr in _shaders)
            {
                var exporter = new ShaderExporter(ptr.Index, (ISerializedFile)ptr.Resource);
                exporter.SaveAs(Path.Combine(folder, 
                    string.IsNullOrWhiteSpace(exporter.FileName) ? ptr.PathID.ToString() : exporter.FileName
                    ), mode);
            }
            foreach (var ptr in _textures)
            {
                var exporter = new TextureExporter(ptr.Index, (ISerializedFile)ptr.Resource);
                exporter.SaveAs(Path.Combine(folder, exporter.FileName), mode);
            }
            foreach (var ptr in _atlases)
            {
                if (!ptr.TryGet(out asset) || !LocationStorage.TryCreate(Path.Combine(folder, asset.Name), ".atlas", mode, out fileName))
                {
                    continue;
                }
                asset.Script.SaveAs(fileName);
            }
        }

        public void Dispose()
        {
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


    }
}
