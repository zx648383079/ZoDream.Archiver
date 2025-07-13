using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class UtageExporter : IMultipartExporter
    {
        public UtageExporter(int entryId, ISerializedFile resource)
        {
            _entryId = entryId;
            _resource = resource;
            _converter = new(resource);
            var obj = _resource[_entryId] as MonoBehaviour;
            Debug.Assert(obj is not null);
            FileName = obj.Name ?? string.Empty;
            Initialize(obj);
        }
        private readonly DocumentReader _converter;
        private readonly int _entryId;
        private readonly ISerializedFile _resource;
        public bool IsEmpty => _data is null || _textureItems.Count == 0;
        public string FileName { get; private set; }
        public IFilePath SourcePath => _resource.FullPath;
        private readonly Dictionary<string, Texture2D> _textureItems = [];
        private UtageDicingBehavior? _data;

        private void Initialize(MonoBehaviour behaviour)
        {
            var data = BehaviorExporter.Deserialize(_entryId, _resource);
            if (!IsIncludeField(data, "cellSize", "padding",
                "textureDataList", "atlasTextures"))
            {
                return;
            }
            _data = _converter.ConvertType<UtageDicingBehavior>(data);
            foreach (var item in _data.AtlasTextures)
            {
                var ptr = _converter.ConvertType<IPPtr<Texture2D>>(item);
                if (ptr is null || !ptr.TryGet(out var texture))
                {
                    continue;
                }
                ptr.IsExclude = true;
                _textureItems.TryAdd(texture.Name ?? string.Empty, texture);
                if (texture.Name?.Contains("Atlas") != true)
                {
                    // 删除
                }
            }
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (IsEmpty || _data is null)
            {
                return;
            }
            var folder = Path.GetDirectoryName(fileName);
            var cellSize = _data.CellSize;
            var padding = _data.Padding;
            foreach (var item in _data.TextureDataList)
            {
                if (!_textureItems.TryGetValue(item.AtlasName, out var texture))
                {
                    continue;
                }
                if (!LocationStorage.TryCreate(
                    Path.Combine(folder, texture.Name), ".u.png", mode, out var outputPath))
                {
                    continue;
                }
                var source = TextureExporter.ToImage(texture, _resource, false);
                if (source is null)
                {
                    continue;
                }
                var contentSize = cellSize - 2 * padding;
                var cols = (int)Math.Ceiling((double)item.Width / contentSize);
                var rows = (int)Math.Ceiling((double)item.Height / contentSize);
                var cellsPerRow = source.Width / cellSize;
                
                using var target = SkiaExtension.MutateImage(item.Width, item.Height, canvas => {
                    canvas.Clear(SKColors.Transparent);

                    for (var y = 0; y < rows; y++)
                    {
                        for (var x = 0; x < cols; x++)
                        {
                            var ci = item.CellIndexList[x + y * cols];
                            if (ci == item.TransparentIndex)
                            {
                                continue;
                            }
                            var cellCol = ci % cellsPerRow;
                            var cellRow = ci / cellsPerRow;
                            var cropWidth = Math.Min(contentSize, item.Width - x * contentSize);
                            var cropHeight = Math.Min(contentSize, item.Height - y * contentSize);
                            var cropX = cellCol * cellSize + padding;
                            var cropY = cellRow * cellSize + padding;
                            // 未翻转
                            // var cropY = source.Height - unityY - cropHeight;

                            canvas.DrawImage(source,
                                SKRect.Create(cropX, cropY, cropWidth, cropHeight),
                                SKRect.Create(x * contentSize, item.Height - (y * contentSize + cropHeight), cropWidth, cropHeight)
                            );
                        }
                    }
                });
                target.SaveAs(outputPath);
            }
        }

        private static bool IsIncludeField(OrderedDictionary? data, params string[] keys)
        {
            if (data == null)
            {
                return false;
            }
            foreach (var key in keys)
            {
                if (!data.Contains(key))
                {
                    return false;
                }
            }
            return true;
        }

        public void Dispose()
        {
            _textureItems.Clear();
        }

        private class UtageDicingBehavior
        {
            public int CellSize { get; set; }
            public int Padding { get; set; }

            public IPPtr<Texture2D>[] AtlasTextures { get; set; }
            public UtageDicingTextureData[] TextureDataList { get; set; }
        }

        private class UtageDicingTextureData
        {
            public string Name { get; set; }
            public string AtlasName { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public int TransparentIndex { get; set; }

            public int[] CellIndexList { get; set; }
        }
    }
}
