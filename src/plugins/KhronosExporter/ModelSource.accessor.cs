using System;
using System.Linq;
using System.Numerics;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Numerics;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace ZoDream.KhronosExporter
{
    public partial class ModelSource
    {
        public void AddAccessorBuffer(int accessorIndex, params int[] value)
        {
            if (Accessors[accessorIndex].ComponentType == EncodingType.UNSIGNED_INT)
            {
                AddAccessorBuffer(accessorIndex, value.Select(i => (uint)i).ToArray());
            } else
            {
                AddAccessorBuffer(accessorIndex, value.Select(i => (ushort)i).ToArray());
            }
        }

        public void AddAccessorBuffer(int accessorIndex, params byte[] value)
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
            var len = sizeof(byte) * value.Length;
            UpdateBufferLength(accessor.BufferView, len);
            accessor.Count += value.Length;
            if (accessor.Min is not null && accessor.Max is not null)
            {
                accessor.Min[0] = Math.Min(accessor.Min[0], value.Min());
                accessor.Max[0] = Math.Max(accessor.Max[0], value.Max());
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
            var len = sizeof(uint) * value.Length;
            UpdateBufferLength(accessor.BufferView, len);
            accessor.Count += value.Length;
            if (accessor.Min is not null && accessor.Max is not null)
            {
                accessor.Min[0] = Math.Min(accessor.Min[0], value.Min());
                accessor.Max[0] = Math.Max(accessor.Max[0], value.Max());
            }
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
            var len = sizeof(ushort) * value.Length;
            UpdateBufferLength(accessor.BufferView, len);
            accessor.Count += value.Length;
            if (accessor.Min is not null && accessor.Max is not null)
            {
                accessor.Min[0] = Math.Min(accessor.Min[0], value.Min());
                accessor.Max[0] = Math.Max(accessor.Max[0], value.Max());
            }
        }

        public void AddAccessorBuffer(int accessorIndex, params float[] value)
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
            var len = sizeof(float) * value.Length;
            UpdateBufferLength(accessor.BufferView, len);
            accessor.Count += value.Length;
            if (accessor.Min is not null && accessor.Max is not null)
            {
                accessor.Min[0] = Math.Min(accessor.Min[0], value.Min());
                accessor.Max[0] = Math.Max(accessor.Max[0], value.Max());
            }
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
            if (accessor.Min is not null && accessor.Max is not null)
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



        /// <summary>
        /// 通过这中方法创建的必须单独一个 bufferView
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public int CreateAccessor<T>(string name, bool hasMinMax = true)
        {
            var sourceType = typeof(T);
            var type = "SCALAR";
            var componentType = EncodingType.FLOAT;
            int? byteStride = null;
            if (sourceType == typeof(byte))
            {
                componentType = EncodingType.BYTE;
            }
            else if (sourceType == typeof(short)) 
            {
                componentType = EncodingType.SHORT;
            }
            else if (sourceType == typeof(ushort))
            {
                componentType = EncodingType.UNSIGNED_SHORT;
            }
            else if (sourceType == typeof(uint))
            {
                componentType = EncodingType.UNSIGNED_INT;
            }
            else if (sourceType == typeof(Vector2))
            {
                byteStride = sizeof(float) * 2;
                type = "VEC2";
            }
            else if (sourceType == typeof(Vector3))
            {
                byteStride = sizeof(float) * 3;
                type = "VEC3";
            }
            else if (sourceType == typeof(Vector4))
            {
                byteStride = sizeof(float) * 4;
                type = "VEC4";
            } else
            {
                throw new NotSupportedException(sourceType.Name);
            }
            var (bufferViewIndex, bufferOffset) = TryCreateBufferView($"{name}_value", () => new BufferView()
            {
                ByteStride = byteStride,
                Target = BufferMode.ARRAY_BUFFER
            });
            var instance = new Accessor
            {
                Name = name,
                Type = type,
                ComponentType = componentType,
                BufferView = bufferViewIndex,
                ByteOffset = bufferOffset
            };
            if (hasMinMax && type.StartsWith("VEC"))
            {
                var len = (int)byteStride! / 4;
                instance.Max = [.. Enumerable.Repeat(float.MinValue, len)];
                instance.Min = [.. Enumerable.Repeat(float.MaxValue, len)];
            }
            return Add(instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        /// <param name="vectorCount"></param>
        /// <param name="isNotVector">比如 /animations/1/samplers/0/output</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public int CreateVectorAccessor(string name, 
            float[] values, int vectorCount, bool isNotVector = false)
        {
            if (vectorCount < 1)
            {
                throw new ArgumentException(nameof(vectorCount));
            }
            var step = values.Length / vectorCount;
            if (step < 2 || step > 4)
            {
                throw new ArgumentException(nameof(step));
            }
            var (bufferViewIndex, bufferOffset) = TryCreateBufferView("Vectors_" + step, () => new BufferView()
            {
                ByteStride = step * 4,
                Target = isNotVector ? null : BufferMode.ARRAY_BUFFER
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
            var index = Add(accessor);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="items"></param>
        /// <param name="hasMinMax">例如 /animations/1/samplers/0/input</param>
        /// <returns></returns>
        public int CreateAccessor(string name, float[] items, bool hasMinMax = false)
        {
            var (bufferViewIndex, bufferOffset) = TryCreateBufferView("Float_", () => new BufferView()
            {
                //ByteStride = hasMinMax ? 4 : null,
                Target = hasMinMax ? null : BufferMode.ARRAY_BUFFER
            });
            var accessor = new Accessor
            {
                Name = name,
                Type = "SCALAR",
                ComponentType = EncodingType.FLOAT,
                BufferView = bufferViewIndex,
                ByteOffset = bufferOffset
            };
            if (hasMinMax)
            {
                accessor.Min = [items.Min()];
                accessor.Max = [items.Max()];
            }
            var index = Add(accessor);
            var writer = OpenWrite(bufferViewIndex);
            foreach (var item in items)
            {
                writer.Write(item);
            }
            var count = items.Length;
            var len = 4 * count;
            UpdateBufferLength(bufferViewIndex, len);
            accessor.Count += count;
            return index;
   
        }
        public int CreateAccessor(string name, Matrix4x4[] items)
        {
            var (bufferViewIndex, bufferOffset) = TryCreateBufferView("Mat4", () => new BufferView()
            {
                Target = BufferMode.ARRAY_BUFFER
            });
            var accessor = new Accessor
            {
                Name = name,
                Type = "MAT4",
                ComponentType = EncodingType.FLOAT,
                BufferView = bufferViewIndex,
                ByteOffset = bufferOffset
            };
            var index = Add(accessor);
            var writer = OpenWrite(bufferViewIndex);
            foreach (var item in items)
            {
                for (int row = 0; row < 4; row++)
                {
                    for (int column = 0; column < 4; column++)
                    {
                        writer.Write(item[row, column]);
                    }
                }
            }
            var count = items.Length;
            var len = 4 * 16 * count;
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
            return Add(new Accessor
            {
                Name = name,
                Type = "SCALAR",
                ComponentType = u32IndicesEnabled ? EncodingType.UNSIGNED_INT : EncodingType.UNSIGNED_SHORT,
                Min = [0f],
                Max = [0f],
                BufferView = bufferViewIndex,
                ByteOffset = bufferOffset
            });
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

}
