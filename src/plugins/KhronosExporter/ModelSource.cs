using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Collections;

namespace ZoDream.KhronosExporter
{
    /// <summary>
    /// 使用验证程序 https://github.khronos.org/glTF-Validator/
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="u32IndicesEnabled"></param>
    public partial class ModelSource(string fileName, bool u32IndicesEnabled = true) : ModelRoot, IDisposable
    {
        public ModelSource()
            : this(string.Empty)
        {
            
        }
        [JsonIgnore]
        public string FileName { get; set; } = fileName;

        [JsonIgnore]
        public Dictionary<string, Stream> ResourceItems { get; private set; } = [];

        #region 添加方法

        public void AddExtensionUsed(string extension)
        {
            var items = ExtensionsUsed ??= [];
            if (!items.Contains(extension))
            {
                items.Add(extension);
            }
        }
        public int Add(Material data)
        {
            return (Materials ??= []).AddWithIndex(data);
        }

        public int Add(BufferView data)
        {
            return BufferViews.AddWithIndex(data);
        }

        public int Add(Image data)
        {
            return (Images ??= []).AddWithIndex(data);
        }

        public int Add(Scene data)
        {
            return (Scenes ??= []).AddWithIndex(data);
        }

        public int Add(Node data)
        {
            return (Nodes ??= []).AddWithIndex(data);
        }

        public int Add(Animation data)
        {
            return (Animations ??= []).AddWithIndex(data);
        }

        public int Add(TextureSampler data)
        {
            return (Samplers ??= []).AddWithIndex(data);
        }

        public int Add(Skin data)
        {
            return (Skins ??= []).AddWithIndex(data);
        }

        public int Add(Texture data)
        {
            return (Textures ??= []).AddWithIndex(data);
        }

        public int Add(Camera data)
        {
            return (Cameras ??= []).AddWithIndex(data);
        }

        public int Add(Mesh data)
        {
            return (Meshes ??= []).AddWithIndex(data);
        }

        public int Add(Accessor data)
        {
            return (Accessors ??= []).AddWithIndex(data);
        }
        #endregion



        public void ResourceClear()
        {
            foreach (var item in ResourceItems)
            {
                item.Value.Dispose();
            }
            ResourceItems.Clear();
        }
        public void Dispose()
        {
            ResourceClear();
        }

      
    }

}
