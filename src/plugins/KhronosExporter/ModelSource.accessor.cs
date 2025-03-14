using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Collections;
using ZoDream.Shared.Numerics;
using Matrix4x4 = ZoDream.Shared.Numerics.Matrix4x4;

namespace ZoDream.KhronosExporter
{
    public partial class ModelSource
    {
        [JsonIgnore]
        private readonly Dictionary<int, AccessorType> _accessorTypeMaps = [];

        public void AddAccessorBuffer(int accessorIndex, params int[] value)
        {
            if (u32IndicesEnabled)
            {
                AddAccessorBuffer(accessorIndex, value.Select(i => (uint)i).ToArray());
            } else
            {
                AddAccessorBuffer(accessorIndex, value.Select(i => (ushort)i).ToArray());
            }
        }
        public void AddAccessorBuffer(int accessorIndex, params uint[] value)
        {
            if (value.Length == 0)
            {
                return;
            }
            var accessor = Accessors[accessorIndex];
            var writer = OpenWrite(accessor.BufferView);
            foreach (var item in value)
            {
                writer.Write(item);
            }
            var len = 4 * value.Length;
            UpdateBufferLength(accessor.BufferView, len);
            accessor.Count += value.Length;
            accessor.Min[0] = Math.Min(accessor.Min[0], value.Min());
            accessor.Max[0] = Math.Max(accessor.Max[0], value.Max());
        }

        public void AddAccessorBuffer(int accessorIndex, params ushort[] value)
        {
            if (value.Length == 0)
            {
                return;
            }
            var accessor = Accessors[accessorIndex];
            var writer = OpenWrite(accessor.BufferView);
            foreach (var item in value)
            {
                writer.Write(item);
            }
            var len = 2 * value.Length;
            UpdateBufferLength(accessor.BufferView, len);
            accessor.Count += value.Length;
            accessor.Min[0] = Math.Min(accessor.Min[0], value.Min());
            accessor.Max[0] = Math.Max(accessor.Max[0], value.Max());
        }

        public void AddAccessorBuffer(int accessorIndex, params Vector3[] value)
        {
            if (value.Length == 0)
            {
                return;
            }
            var accessor = Accessors[accessorIndex];
            var writer = OpenWrite(accessor.BufferView);
            foreach (var item in value)
            {
                writer.Write(item.X);
                writer.Write(item.Y);
                writer.Write(item.Z);
            }
            var len = 12 * value.Length;
            UpdateBufferLength(accessor.BufferView, len);
            accessor.Count += value.Length;
            if (_accessorTypeMaps.TryGetValue(accessorIndex, out var type) && type == AccessorType.Position)
            {
                accessor.Min[0] = Math.Min(accessor.Min[0], value.Select(i => i.X).Min());
                accessor.Min[1] = Math.Min(accessor.Min[1], value.Select(i => i.Y).Min());
                accessor.Min[2] = Math.Min(accessor.Min[2], value.Select(i => i.Z).Min());
                accessor.Max[0] = Math.Max(accessor.Max[0], value.Select(i => i.X).Max());
                accessor.Max[1] = Math.Max(accessor.Max[1], value.Select(i => i.Y).Max());
                accessor.Max[2] = Math.Max(accessor.Max[2], value.Select(i => i.Z).Max());
            }
        }

        public void AddAccessorBuffer(int accessorIndex, params Vector2[] value)
        {
            if (value.Length == 0)
            {
                return;
            }
            var accessor = Accessors[accessorIndex];
            var writer = OpenWrite(accessor.BufferView);
            foreach (var item in value)
            {
                writer.Write(item.X);
                writer.Write(item.Y);
            }
            var len = 8 * value.Length;
            accessor.Count += value.Length;
            UpdateBufferLength(accessor.BufferView, len);
        }

        public int CreateNormalAccessor(string name)
        {
            var (bufferViewIndex, bufferOffset) = TryCreateBufferView("Normals", () => new BufferView()
            {
                ByteStride = 12,
            });
            return AddAccessor(new Accessor
            {
                Name = name,
                Type = "VEC3",
                ComponentType = EncodingType.FLOAT,
                BufferView = bufferViewIndex,
                ByteOffset = bufferOffset
            }, AccessorType.Normal);
        }

        public void AddNormalAccessorBuffer(int accessorIndex, params Vector3[] value)
        {
            AddAccessorBuffer(accessorIndex, value);
        }

        public int CreatePositionAccessor(string name)
        {
            var (bufferViewIndex, bufferOffset) = TryCreateBufferView("Positions", () => new BufferView()
            {
                ByteStride = 12,
                Target = BufferMode.ARRAY_BUFFER
            });
            return AddAccessor(new Accessor
            {
                Name = name,
                Min = [float.MaxValue, float.MaxValue, float.MaxValue],   // any number must be smaller
                Max = [float.MinValue, float.MinValue, float.MinValue],   // any number must be bigger
                Type = "VEC3",
                ComponentType = EncodingType.FLOAT,
                BufferView = bufferViewIndex,
                ByteOffset = bufferOffset
            }, AccessorType.Position);
        }

        public void AddPositionAccessorBuffer(int accessorIndex, params Vector3[] value)
        {
            AddAccessorBuffer(accessorIndex, value);
        }

        public int CreateUvAccessor(string name)
        {
            var (bufferViewIndex, bufferOffset) = TryCreateBufferView("Uvs", () => new BufferView()
            {
                ByteStride = 8,
            });
            return AddAccessor(new Accessor
            {
                Name = name,
                Type = "VEC2",
                ComponentType = EncodingType.FLOAT,
                BufferView = bufferViewIndex,
                ByteOffset = bufferOffset
            }, AccessorType.Uv);
        }

        public void AddPositionAccessorBuffer(int accessorIndex, params Vector2[] value)
        {
            AddAccessorBuffer(accessorIndex, value);
        }

        public int CreateVectorAccessor(string name, float[] values, int vectorCount)
        {
            var step = values.Length / vectorCount;
            if (step < 2 || step > 4)
            {
                throw new ArgumentException();
            }
            var (bufferViewIndex, bufferOffset) = TryCreateBufferView("Vectors_" + step, () => new BufferView()
            {
                ByteStride = step * 4,
                Target = BufferMode.ARRAY_BUFFER
            });
            var accessor = new Accessor
            {
                Name = name,
                Type = "VEC" + step,
                ComponentType = EncodingType.FLOAT,
                Min = [.. Enumerable.Repeat(float.MaxValue, step)],
                Max = [.. Enumerable.Repeat(float.MinValue, step)],
                BufferView = bufferViewIndex,
                ByteOffset = bufferOffset
            };
            var index = AddAccessor(accessor, AccessorType.Uv);
            var writer = OpenWrite(bufferViewIndex);
            for (int i = 0; i < vectorCount; i++)
            {
                var begin = i * step;
                for (var j = 0; j < step; j++)
                {
                    var val = values[begin + j];
                    writer.Write(val);
                    if (val < accessor.Min[j])
                    {
                        accessor.Min[j] = val;
                    }
                    if (val > accessor.Max[j])
                    {
                        accessor.Max[j] = val;
                    }
                }
            }
            var count = vectorCount;
            var len = 4 * count * step;
            UpdateBufferLength(bufferViewIndex, len);
            accessor.Count += count;
            return index;
        }

        public int CreateIndicesAccessor(string name)
        {
            var (bufferViewIndex, bufferOffset) = TryCreateBufferView("Indices", () => new BufferView()
            {
                // ByteStride = u32IndicesEnabled ? 4 : 2,
                // Buffer = TryCreateBuffer("Indexes"),
                Target = BufferMode.ELEMENT_ARRAY_BUFFER,
            });
            return AddAccessor(new Accessor
            {
                Name = name,
                Type = "SCALAR",
                ComponentType = u32IndicesEnabled ? EncodingType.UNSIGNED_INT : EncodingType.UNSIGNED_SHORT,
                Min = [0f],
                Max = [0f],
                BufferView = bufferViewIndex,
                ByteOffset = bufferOffset
            }, AccessorType.Indices);
        }

        public void AddIndicesAccessorBuffer(int accessorIndex, params int[] value)
        {
            AddAccessorBuffer(accessorIndex, value);
        }
        public int CreateAccessor(string name, AccessorType type)
        {
            return type switch
            {
                AccessorType.Normal => CreateNormalAccessor(name),
                AccessorType.Position => CreatePositionAccessor(name),
                AccessorType.Uv => CreateUvAccessor(name),
                AccessorType.Indices => CreateIndicesAccessor(name),
                _ => -1
            };
        }

        private int AddAccessor(Accessor accessor, AccessorType type)
        {
            var index = Accessors.AddWithIndex(accessor);
            _accessorTypeMaps.Add(index, type);
            return index;
        }

        public object? ReadAccessorBuffer(Accessor accessor)
        {
            var bufferView = BufferViews[accessor.BufferView];
            var stream = GetStream(accessor.BufferView);
            stream.Position = bufferView.ByteOffset + accessor.ByteOffset;
            var reader = new BundleBinaryReader(stream);
            return accessor.Type switch
            {
                "SCALAR" => accessor.ComponentType switch
                {
                    EncodingType.BYTE or EncodingType.UNSIGNED_BYTE => reader.ReadBytes(accessor.Count),
                    EncodingType.SHORT => reader.ReadArray(accessor.Count, (r, _) => r.ReadInt16()),
                    EncodingType.UNSIGNED_SHORT => reader.ReadArray(accessor.Count, (r, _) => r.ReadUInt16()),
                    EncodingType.UNSIGNED_INT => reader.ReadArray(accessor.Count, (r, _) => r.ReadInt32()),
                    EncodingType.FLOAT => reader.ReadArray(accessor.Count, (r, _) => r.ReadSingle()),
                    _ => null,
                },
                "VEC2" => reader.ReadArray(accessor.Count, (r, _) => new Vector2(r.ReadSingle(), r.ReadSingle())),
                "VEC3" => reader.ReadArray(accessor.Count, (r, _) => new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle())),
                "VEC4" => reader.ReadArray(accessor.Count, (r, _) => new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle())),
                "MAT2" => reader.ReadArray(accessor.Count, (r, _) => new Matrix2x2(
                    r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadSingle())),
                "MAT3" => reader.ReadArray(accessor.Count, (r, _) => new Matrix3x3(
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle())),
                "MAT4" => reader.ReadArray(accessor.Count, (r, _) => new Matrix4x4(
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle())),
                _ => null,
            };
        }
    }

    public enum AccessorType
    {
        Normal,
        Position,
        Uv,
        Indices
    }
}
