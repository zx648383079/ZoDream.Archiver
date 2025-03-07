using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Models;

namespace ZoDream.KhronosExporter
{
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
