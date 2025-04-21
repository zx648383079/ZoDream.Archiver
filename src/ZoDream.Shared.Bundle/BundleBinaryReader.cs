using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Reflection.PortableExecutable;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public class BundleBinaryReader(
        Stream stream, 
        EndianType endian = EndianType.LittleEndian,
        bool isAlignStream = false,
        bool leaveOpen = true)
        : EndianReader(stream, endian, leaveOpen), IBundleBinaryReader
    {

        public BundleBinaryReader(Stream stream, IBundleBinaryReader parent, bool leaveOpen = true)
            : this (stream, parent.EndianType, parent, leaveOpen)
        {
            
        }

        public BundleBinaryReader(Stream stream, EndianType endian, IBundleBinaryReader parent, bool leaveOpen = true)
            : this(stream, endian, parent.IsAlignStream, leaveOpen)
        {
            CopyFrom(parent);
        }

        public bool IsAlignStream { get; private set; } = isAlignStream;

        public bool LeaveStreamOpen { get; set; } = leaveOpen;

        #region 附加信息存储
        public Dictionary<string, object> Items { get; private set; } = [];

        public void Add(string name, object value)
        {
            if (Items.TryAdd(name, value))
            {
                return;
            }
            Items[name] = value;
        }

        public void Add<T>(T value)
        {
            Add(typeof(T).Name, value);
        }

        public T Get<T>(string name)
        {
            TryGet<T>(name, out var value);
            return value;
        }

        public T Get<T>()
        {
            return Get<T>(typeof(T).Name);
        }

        public bool TryGet<T>(string name, [NotNullWhen(true)] out T? instance)
        {
            if (Items.TryGetValue(name, out var value))
            {
                instance = (T)value;
                return true;
            }
            instance = default;
            return false;
        }
        public bool TryGet<T>([NotNullWhen(true)] out T? instance)
        {
            return TryGet(typeof(T).Name, out instance);
        }
        /// <summary>
        /// 继承其他的类型
        /// </summary>
        /// <param name="parent"></param>
        public void CopyFrom(IBundleBinaryReader parent)
        {
            if (parent is BundleBinaryReader i)
            {
                foreach (var item in i.Items)
                {
                    Items.Add(item.Key, item.Value);
                }
            }
        }

        #endregion
        public void AlignStream()
        {
            AlignStream(4);
        }

        public void AlignStream(int alignment)
        {
            var pos = Position;
            var mod = pos % alignment;
            if (mod != 0)
            {
                BaseStream.Seek(alignment - mod, SeekOrigin.Current);
            }
        }

        public T AlignStream<T>(Func<IBundleBinaryReader, T> cb, int alignment = 4)
        {
            var res = cb.Invoke(this);
            if (IsAlignStream && alignment > 0)
            {
                AlignStream(alignment);
            }
            return res;
        }



        public T[][] Read2DArray<T>(Func<IBundleBinaryReader, int, int, T> cb)
        {
            return ReadArray((_, i) => {
                return ReadArray((_, j) => {
                    return cb.Invoke(this, i, j);
                });
            });
        }

        public T[][] Read2DArray<T>(Func<IBundleBinaryReader, T> cb) 
        {
            return Read2DArray((r, _, _) => cb.Invoke(r));
        }

        public T[][] Read2DArray<T>(IBundleSerializer serializer)
        {
            return Read2DArray((r, _, _) => serializer.Deserialize<T>(r));
        }

        public string ReadAlignedString()
        {
            var res = ReadString();
            AlignStream();
            return res;
        }

        public T[] ReadArray<T>(int count, Func<IBundleBinaryReader, int, T> cb)
        {
            if (count <= 0)
            {
                return [];
            }
            var items = new T[count];
            for (int i = 0; i < count; i++)
            {
                items[i] = cb.Invoke(this, i);
            }
            return items;
        }

        public T[] ReadArray<T>(int count, Func<IBundleBinaryReader, T> cb)
        {
            return ReadArray(count, (r, _) => cb.Invoke(r));
        }

        public T[] ReadArray<T>(Func<IBundleBinaryReader, int, T> cb)
        {
            return ReadArray(ReadInt32(), cb);
        }
        public T[] ReadArray<T>(Func<IBundleBinaryReader, T> cb)
        {
            return ReadArray(ReadInt32(), (_, _) => cb.Invoke(this));
        }
        public T[] ReadArray<T>(Func<T> cb)
        {
            return ReadArray(ReadInt32(), (_, _) => cb.Invoke());
        }

        public T[] ReadArray<T>(IBundleSerializer serializer)
        {
            return ReadArray(serializer.Deserialize<T>);
        }

        public T[] ReadArray<T>(int count, IBundleSerializer serializer)
        {
            return ReadArray(count, serializer.Deserialize<T>);
        }
        public void ReadArray(int count, Action<IBundleBinaryReader, int> cb)
        {
            for (var i = 0; i < count; i++)
            {
                cb.Invoke(this, i);
            }
        }

        public void ReadArray(Action<IBundleBinaryReader, int> cb)
        {
            ReadArray(ReadInt32(), cb);
        }

        public Vector2 ReadVector2()
        {
            return new(ReadSingle(), ReadSingle());
        }
        public Vector3 ReadVector3()
        {
            return new(ReadSingle(), ReadSingle(), ReadSingle());
        }
        public Vector4 ReadVector4()
        {
            return new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }
        public Quaternion ReadQuaternion()
        {
            return new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }

        public Matrix4x4 ReadMatrix()
        {
            var data = new Matrix4x4();
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    data[r, c] = ReadSingle();
                }
            }
            return data;
        }

        public Stream ReadAsStream()
        {
            return ReadAsStream(ReadInt32());
        }
        public Stream ReadAsStream(long length)
        {
            return new PartialStream(BaseStream, length);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (LeaveStreamOpen == false)
            {
                BaseStream.Dispose();
            }
        }
    }
}
