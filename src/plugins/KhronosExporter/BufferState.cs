using System;
using System.IO;
using System.Numerics;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Collections;

namespace ZoDream.KhronosExporter
{
    public class BufferState: IDisposable
    {
        private bool disposedValue = false; // To detect redundant calls
        private readonly ModelRoot _model;
        private readonly string _fileNameWithoutExtension;
        private readonly string _folder;
        private readonly bool _u32IndicesEnabled;

        /// <summary>
        /// This assumes the model is not already populated by buffers
        /// </summary>
        public BufferState(ModelRoot model, string fileName, bool u32IndicesEnabled)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            _fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            _folder = Path.GetDirectoryName(fileName);
            _u32IndicesEnabled = u32IndicesEnabled;
        }

        public BinaryWriter PositionsStream { get; private set; }
        public BufferSource PositionsBuffer { get; private set; }
        public BufferView PositionsBufferView { get; private set; }
        public int? PositionsBufferViewIndex { get; private set; }
        public Accessor CurrentPositionsAccessor { get; private set; }

        public BinaryWriter NormalsStream { get; private set; }
        public BufferSource NormalsBuffer { get; private set; }
        public BufferView NormalsBufferView { get; private set; }
        public int? NormalsBufferViewIndex { get; private set; }
        public Accessor CurrentNormalsAccessor { get; private set; }

        public BinaryWriter UvsStream { get; private set; }
        public BufferSource UvsBuffer { get; private set; }
        public BufferView UvsBufferView { get; private set; }
        public int? UvsBufferViewIndex { get; private set; }
        public Accessor CurrentUvsAccessor { get; private set; }

        public BinaryWriter IndicesStream { get; private set; }
        public BufferSource IndicesBuffer { get; private set; }
        public BufferView IndicesBufferView { get; private set; }
        public int? IndicesBufferViewIndex { get; private set; }
        public Accessor CurrentIndicesAccessor { get; private set; }

        public void AddPosition(Vector3 position)
        {
            var accessor = CurrentPositionsAccessor;
            AddToBuffer(position, PositionsStream, PositionsBuffer, PositionsBufferView, accessor);
            accessor.Min[0] = position.X < accessor.Min[0] ? position.X : accessor.Min[0];
            accessor.Min[1] = position.Y < accessor.Min[1] ? position.Y : accessor.Min[1];
            accessor.Min[2] = position.Z < accessor.Min[2] ? position.Z : accessor.Min[2];

            accessor.Max[0] = position.X > accessor.Max[0] ? position.X : accessor.Max[0];
            accessor.Max[1] = position.Y > accessor.Max[1] ? position.Y : accessor.Max[1];
            accessor.Max[2] = position.Z > accessor.Max[2] ? position.Z : accessor.Max[2];
        }

        internal void AddNormal(Vector3 normal)
        {
            AddToBuffer(normal, NormalsStream, NormalsBuffer, NormalsBufferView, CurrentNormalsAccessor);
        }
        internal void AddUv(Vector2 uv)
        {
            AddToBuffer(uv, UvsStream, UvsBuffer, UvsBufferView, CurrentUvsAccessor);
        }
        internal void AddIndex(int index)
        {
            var accessor = CurrentIndicesAccessor;
            if (_u32IndicesEnabled)
            {
                AddToBuffer(
                    (uint)index,
                    IndicesStream,
                    IndicesBuffer,
                    IndicesBufferView,
                    accessor);
            }
            else
            {
                AddToBuffer(
                    (ushort)index,
                    IndicesStream,
                    IndicesBuffer,
                    IndicesBufferView,
                    accessor);
            }

            accessor.Min[0] = index < accessor.Min[0] ? index : accessor.Min[0];
            accessor.Max[0] = index > accessor.Max[0] ? index : accessor.Max[0];
        }

        private void AddToBuffer(Vector2 value, BinaryWriter sw, BufferSource buffer, BufferView bufferview, Accessor accessor)
        {
            sw.Write(value.X);
            sw.Write(value.Y);
            buffer.ByteLength += 8;
            bufferview.ByteLength = buffer.ByteLength;
            accessor.Count++;
        }

        private void AddToBuffer(Vector3 value, BinaryWriter sw, BufferSource buffer, BufferView bufferview, Accessor accessor)
        {
            sw.Write(value.X);
            sw.Write(value.Y);
            sw.Write(value.Z);
            buffer.ByteLength += 12;
            bufferview.ByteLength = buffer.ByteLength;
            accessor.Count++;
        }

        private void AddToBuffer(ushort value, BinaryWriter sw, BufferSource buffer, BufferView bufferview, Accessor accessor)
        {
            sw.Write(value);
            buffer.ByteLength += 2;
            bufferview.ByteLength = buffer.ByteLength;
            accessor.Count++;
        }

        private void AddToBuffer(uint value, BinaryWriter sw, BufferSource buffer, BufferView bufferview, Accessor accessor)
        {
            sw.Write(value);
            buffer.ByteLength += 4;
            bufferview.ByteLength = buffer.ByteLength;
            accessor.Count++;
        }

        internal int MakePositionAccessor(string name)
        {
            if (PositionsBufferView == null)
            {
                var positionsFile = _fileNameWithoutExtension + "_Positions.bin";
                var stream = File.Create(Path.Combine(_folder, positionsFile));
                PositionsStream = new BinaryWriter(stream);
                PositionsBuffer = new BufferSource() { Name = "Positions", Uri = positionsFile };
                PositionsBufferView = new BufferView
                {
                    Name = "Positions",
                    ByteStride = 12,
                    Buffer = _model.Buffers.AddWithIndex(PositionsBuffer),
                    Target = BufferMode.ARRAY_BUFFER
                };
                PositionsBufferViewIndex = _model.BufferViews.AddWithIndex(PositionsBufferView);
            }
            CurrentPositionsAccessor = new Accessor
            {
                Name = name,
                Min = [float.MaxValue, float.MaxValue, float.MaxValue],   // any number must be smaller
                Max = [float.MinValue, float.MinValue, float.MinValue],   // any number must be bigger
                Type = "VEC3",
                ComponentType = EncodingType.FLOAT,
                BufferView = PositionsBufferViewIndex.Value,
                ByteOffset = PositionsBuffer.ByteLength
            };
            return _model.Accessors.AddWithIndex(CurrentPositionsAccessor);
        }

        public int MakeNormalAccessors(string name)
        {
            if (NormalsBufferView == null)
            {
                var normalsFileName = _fileNameWithoutExtension + "_Normals.bin";
                var stream = File.Create(Path.Combine(_folder, normalsFileName));
                NormalsStream = new BinaryWriter(stream);
                NormalsBuffer = new BufferSource() { Name = "Normals", Uri = normalsFileName };
                NormalsBufferView = new BufferView
                {
                    Name = "Normals",
                    Buffer = _model.Buffers.AddWithIndex(NormalsBuffer),
                    ByteStride = 12,
                    //Target = BufferViewTarget.ARRAY_BUFFER
                };
                NormalsBufferViewIndex = _model.BufferViews.AddWithIndex(NormalsBufferView);
            }
            CurrentNormalsAccessor = new Accessor
            {
                Name = name,
                //Min = new Single[] { 0, 0, 0 },
                //Max = new Single[] { 0, 0, 0 },
                Type = "VEC3",
                ComponentType = EncodingType.FLOAT,
                BufferView = NormalsBufferViewIndex.Value,
                ByteOffset = NormalsBuffer.ByteLength
            };
            return _model.Accessors.AddWithIndex(CurrentNormalsAccessor);
        }

        internal int MakeUvAccessor(string name)
        {
            if (UvsBufferView == null)
            {
                var UvsFileName = _fileNameWithoutExtension + "_Uvs.bin";
                var stream = File.Create(Path.Combine(_folder, UvsFileName));
                UvsStream = new BinaryWriter(stream);
                UvsBuffer = new BufferSource() { Name = "Uvs", Uri = UvsFileName };
                UvsBufferView = new BufferView
                {
                    Name = "Uvs",
                    Buffer = _model.Buffers.AddWithIndex(UvsBuffer),
                    ByteStride = 8,
                    //Target = BufferViewTarget.ARRAY_BUFFER
                };
                UvsBufferViewIndex = _model.BufferViews.AddWithIndex(UvsBufferView);
            }
            CurrentUvsAccessor = new Accessor
            {
                Name = name,
                //Min = new Single[] { 0, 0 },
                //Max = new Single[] { 0, 0 },
                Type = "VEC2",
                ComponentType = EncodingType.FLOAT,
                BufferView = UvsBufferViewIndex.Value,
                ByteOffset = UvsBuffer.ByteLength
            };
            return _model.Accessors.AddWithIndex(CurrentUvsAccessor);
        }

        internal int MakeIndicesAccessor(string name)
        {
            if (IndicesBufferView == null)
            {
                var indicessFileName = _fileNameWithoutExtension + "_Indices.bin";
                var stream = File.Create(Path.Combine(_folder, indicessFileName));
                IndicesStream = new BinaryWriter(stream);
                IndicesBuffer = new BufferSource() { Name = "Indices", Uri = indicessFileName };
                IndicesBufferView = new BufferView()
                {
                    Name = "Indexes",
                    //ByteStride = _u32IndicesEnabled ? 8 : 4,
                    Buffer = _model.Buffers.AddWithIndex(IndicesBuffer),
                    Target = BufferMode.ELEMENT_ARRAY_BUFFER,
                };
                IndicesBufferViewIndex = _model.BufferViews.AddWithIndex(IndicesBufferView);
            }

            CurrentIndicesAccessor = new Accessor
            {
                Name = name,
                Type = "SCALAR",
                ComponentType = _u32IndicesEnabled ? EncodingType.UNSIGNED_INT : EncodingType.UNSIGNED_SHORT,
                BufferView = IndicesBufferViewIndex.Value,
                ByteOffset = IndicesBuffer.ByteLength,
                Min = [0f],
                Max = [0f]
            };
            return _model.Accessors.AddWithIndex(CurrentIndicesAccessor);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    PositionsStream?.Dispose();
                    NormalsStream?.Dispose();
                    UvsStream?.Dispose();
                    IndicesStream?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}
