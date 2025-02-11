using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZoDream.KhronosExporter
{
    public class KhronosScheme : IEntryScheme<ModelRoot>
    {
        public async Task CreateAsync(IStorageFileEntry entry, ModelRoot data, IArchiveOptions? options = null)
        {
            if (entry.HasExtension(".glb"))
            {
                new GlbWriter().Write(data, await entry.OpenWriteAsync());
            } else if (entry.HasExtension(".gltf"))
            {
                new GltfWriter().Write(data, await entry.OpenWriteAsync());
            }
        }

        public IEntryReader? GetReader(Stream input, IStorageFileEntry entry)
        {
            if (entry.HasExtension(".glb"))
            {
                return new GlbReader();
            }
            else if (entry.HasExtension(".gltf"))
            {
                return new GltfReader();
            } else if (entry.HasExtension("obj"))
            {
                return new ObjReader();
            }
            return null;
        }

        public IEntryWriter? GetWriter(IStorageFileEntry entry)
        {
            if (entry.HasExtension(".glb"))
            {
                return new GlbWriter();
            }
            else if (entry.HasExtension(".gltf"))
            {
                return new GltfWriter();
            }
            return null;
        }

        public async Task<ModelRoot?> OpenAsync(IStorageFileEntry entry, IArchiveOptions? options = null)
        {
            if (entry.HasExtension(".glb"))
            {
                return await new GlbReader().ReadAsync(entry);
            }
            else if (entry.HasExtension(".gltf"))
            {
                return await new GltfReader().ReadAsync(entry);
            }
            else if (entry.HasExtension("obj"))
            {
                return await new ObjReader().ReadAsync(entry);
            }
            return null;
        }
    }
}
