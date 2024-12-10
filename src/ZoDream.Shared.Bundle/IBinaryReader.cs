using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleBinaryReader: IDisposable
    {
        public Stream BaseStream { get; }

        public bool LeaveStreamOpen { get; set; }

        public long Length { get; }
        public long Position { get; set; }
        /// <summary>
        /// 剩余字节长度
        /// </summary>
        public long RemainingLength { get; }
        public EndianType EndianType { get; }
        public bool IsAlignStream { get; }

        #region 默认BinaryReader 功能
        /// <summary>
        /// 读取7位压缩的 uint
        /// </summary>
        /// <returns></returns>
        public int Read7BitEncodedInt();
        /// <summary>
        /// 读取7位压缩的 ulong
        /// </summary>
        /// <returns></returns>
        public long Read7BitEncodedInt64();
        public bool ReadBoolean();
        public byte ReadByte();

        public int Read(byte[] buffer, int index, int count);
        public byte[] ReadBytes(int count);
        public decimal ReadDecimal();
        public double ReadDouble();
        public Half ReadHalf();
        public short ReadInt16();
        public int ReadInt32();
        public long ReadInt64();
        public sbyte ReadSByte();
        public float ReadSingle();
        public string ReadString();
        public ushort ReadUInt16();
        public uint ReadUInt32();
        public ulong ReadUInt64();
        #endregion

        #region 拓展BinaryReader的功能
        /// <summary>
        /// 读取 string 直到 0x0 停止
        /// </summary>
        /// <returns></returns>
        public string ReadStringZeroTerm();
        /// <summary>
        /// 读取 string 直到 0x0 停止
        /// </summary>
        /// <returns></returns>
        public string ReadStringZeroTerm(int maxLength);
        /// <summary>
        /// 读取 string 直到 0x0 停止
        /// </summary>
        /// <returns></returns>
        public bool ReadStringZeroTerm(int maxLength, [NotNullWhen(true)] out string? result);
        public string ReadString(int length);
        public string ReadAlignedString();
        public T[] ReadArray<T>(int count, Func<IBundleBinaryReader, int, T> cb);
        public T[] ReadArray<T>(Func<IBundleBinaryReader, int, T> cb);
        public T[] ReadArray<T>(Func<IBundleBinaryReader, T> cb);
        public T[] ReadArray<T>(Func<T> cb);
        /// <summary>
        /// 获取二维数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cb"></param>
        /// <returns></returns>
        public T[][] Read2DArray<T>(Func<IBundleBinaryReader, int, int, T> cb);

        public void ReadArray(int count, Action<IBundleBinaryReader, int> cb);
        public void ReadArray(Action<IBundleBinaryReader, int> cb);

        public void ReadExactly(Span<byte> buffer);

        public Stream ReadAsStream();
        public Stream ReadAsStream(long length);

        public void AlignStream();
        public void AlignStream(int alignment);
        /// <summary>
        /// 执行之后应用AlignStream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cb"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
        public T AlignStream<T>(Func<IBundleBinaryReader, T> cb, int alignment = 4);
        
        #endregion
        #region 设置获取一些已知信息
        public void Add(string name, object value);
        public void Add<T>(T value);

        public T Get<T>(string name);
        public T Get<T>();
        public bool TryGet<T>(string name, [NotNullWhen(true)] out T? instance);
        public bool TryGet<T>([NotNullWhen(true)] out T? instance);
        
        #endregion

    }
}
