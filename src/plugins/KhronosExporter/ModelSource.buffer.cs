using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Collections;
using ZoDream.Shared.IO;

namespace ZoDream.KhronosExporter
{
    public partial class ModelSource
    {
        [JsonIgnore]
        private readonly Dictionary<int, Stream> _bufferViewCacheItems = [];

        [JsonIgnore]
        private readonly Dictionary<string, int> _bufferViewMaps = [];
        private Stream GetStream(int bufferViewIndex)
        {
            if (_bufferViewCacheItems.TryGetValue(bufferViewIndex, out var stream))
            {
                return stream;
            }
            var buffer = BufferViews[bufferViewIndex];
            if (buffer.Buffer >= 0)
            {
                return GetStream(Buffers[BufferViews[bufferViewIndex].Buffer]);
            }
            stream = new MemoryStream();
            _bufferViewCacheItems.Add(bufferViewIndex, stream);
            buffer.ByteOffset = 0;
            buffer.ByteLength = 0;
            return stream;
        }
        public BinaryWriter OpenWrite(int bufferViewIndex)
        {
            return new BinaryWriter(GetStream(bufferViewIndex));
        }

        private (int, int) TryCreateBufferView(string name, Func<BufferView> cb)
        {
            if (_bufferViewMaps.TryGetValue(name, out var bufferViewIndex))
            {
                return (bufferViewIndex, BufferViews[bufferViewIndex].ByteLength);
            }
            var buffer = cb.Invoke();
            buffer.Name = name;
            bufferViewIndex = BufferViews.AddWithIndex(buffer);
            _bufferViewMaps.Add(name, bufferViewIndex);
            return (bufferViewIndex, buffer.ByteLength);
        }

        private void UpdateBufferLength(int bufferViewIndex, int length)
        {
            var bufferView = BufferViews[bufferViewIndex];
            bufferView.ByteLength += length;
            if (bufferView.Buffer >= 0)
            {
                Buffers[bufferView.Buffer].ByteLength += length;
            }
        }

        /// <summary>
        /// 合并 bufferView 的缓存数据
        /// </summary>
        /// <param name="bufferIndex"></param>
        /// <param name="output"></param>
        public void FlushBuffer(int bufferIndex, Stream output)
        {
            var buffer = Buffers[bufferIndex];
            foreach (var item in _bufferViewCacheItems)
            {
                var view = BufferViews[item.Key];
                view.Buffer = bufferIndex;

                var pos = (int)output.Position;
                item.Value.CopyTo(output, view.ByteOffset, view.ByteLength);
                view.ByteOffset = pos;
                item.Value.Dispose();
            }
            output.Flush();
            buffer.ByteLength = (int)output.Length;
            _bufferViewCacheItems.Clear();
            _bufferViewMaps.Clear();
        }

        public void FlushBuffer()
        {
            if (Buffers.Count == 0)
            {
                Buffers.Add(new BufferSource());
            }
            var buffer = Buffers[0];
            var stream = GetStream(buffer);
            stream.Position = buffer.ByteLength;
            FlushBuffer(0, stream);
        }
        public Stream GetStream(BufferSource buffer)
        {
            var name = string.IsNullOrWhiteSpace(buffer.Uri) || string.IsNullOrEmpty(FileName) ? string.Empty : buffer.Uri;
            if (!string.IsNullOrEmpty(buffer.Uri) && buffer.Uri.StartsWith("data:"))
            {
                name = $"@@b_{buffer.Name}";
            }
            if (ResourceItems.TryGetValue(name, out var stream))
            {
                return stream;
            }
            stream = OpenStream(buffer.Uri);
            if (stream is null)
            {
                stream = new MemoryStream();
                buffer.Uri = null;
            }
            ResourceItems.Add(name, stream);
            return stream;
        }


        public Stream? OpenStream(string uri)
        {
            if (string.IsNullOrEmpty(FileName))
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(uri))
            {
                return ResourceItems.TryGetValue(string.Empty, out var stream) ? stream : null;
            }
            if (TryDecodeBase64String(uri, out var buffer))
            {
                return new MemoryStream(buffer, true);
            }
            return File.Open(Path.Combine(Path.GetDirectoryName(FileName), uri), FileMode.OpenOrCreate);
        }
        public BinaryWriter OpenWrite(BufferSource buffer)
        {
            return new BinaryWriter(GetStream(buffer));
        }

        public BinaryReader OpenRead(BufferSource buffer)
        {
            return new BinaryReader(GetStream(buffer));
        }

        private int TryCreateBuffer(string name)
        {
            for (int i = 0; i < Buffers.Count; i++)
            {
                if (Buffers[i].Name == name)
                {
                    return i;
                }
            }
            var buffer = new BufferSource()
            {
                Name = name,
                Uri = $"{Path.GetFileNameWithoutExtension(FileName)}_{name}.bin"
            };
            return Buffers.AddWithIndex(buffer);
        }

        internal static bool TryDecodeBase64String(string uri, [NotNullWhen(true)] out byte[]? buffer)
        {
            if (!uri.StartsWith("data:"))
            {
                buffer = null;
                return false;
            }
            var i = uri.IndexOf(";base64,");
            if (i < 0)
            {
                buffer = null;
                return false;
            }
            buffer = Convert.FromBase64String(uri[(i + 8)..]);
            return true;
        }
    }

}
