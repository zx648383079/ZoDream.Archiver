using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerElementArray : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Add@FbxLayerElementArray@fbxsdk@@QEAAHPEBXW4EFbxType@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        internal static extern int AddInternal(nint InHandle, nint pItem, EFbxType pValueType);


        [DllImport(NativeMethods.DllName, EntryPoint = "?GetCount@FbxLayerElementArray@fbxsdk@@QEBAHXZ")]
        internal static extern int GetCountInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetAt@FbxLayerElementArray@fbxsdk@@QEBA_NHPEAPEAXW4EFbxType@2@@Z")]
        internal static extern bool GetAtInternal(nint handle, int pIndex, nint pItem, EFbxType pValueType);

        public int Count => GetCountInternal(Handle);

        public FbxLayerElementArray(nint inHandle)
            : base(inHandle)
        {
        }

        public int Add(double x, double y, double z, double w = 0.0)
        {
            using var vec = new FbxDouble4(x, y, z, w);
            return AddInternal(Handle, vec.Handle, EFbxType.eFbxDouble4);
        }

        public int Add(double x, double y)
        {
            using var vec = new FbxDouble2(x, y);
            return AddInternal(Handle, vec.Handle, EFbxType.eFbxDouble2);
        }

        public int Add(int a)
        {
            nint ptr = FbxUtils.FbxMalloc(4);
            Marshal.WriteInt32(ptr, 0, a);
            int idx = AddInternal(Handle, ptr, EFbxType.eFbxInt);
            FbxUtils.FbxFree(ptr);
            return idx;
        }

        public void GetAt(int index, out Vector4 outValue)
        {
            outValue = new Vector4();
            nint ptr = GetAt(index, EFbxType.eFbxDouble4);

            outValue.X = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 0));
            outValue.Y = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 8));
            outValue.Z = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 16));
            outValue.W = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 24));
            FbxUtils.FbxFree(ptr);
        }

        public void GetAt(int index, out Vector3 outValue)
        {
            outValue = new Vector3();
            nint ptr = GetAt(index, EFbxType.eFbxDouble3);

            outValue.X = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 0));
            outValue.Y = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 8));
            outValue.Z = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 16));
            FbxUtils.FbxFree(ptr);
        }

        public void GetAt(int index, out Vector2 outValue)
        {
            outValue = new Vector2();
            nint ptr = GetAt(index, EFbxType.eFbxDouble2);

            outValue.X = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 0));
            outValue.Y = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 8));
            FbxUtils.FbxFree(ptr);
        }

        public void GetAt(int index, out int outValue)
        {
            nint ptr = GetAt(index, EFbxType.eFbxInt);
            outValue = Marshal.ReadInt32(ptr);
            FbxUtils.FbxFree(ptr);
        }

        private unsafe nint GetAt(int index, EFbxType type)
        {
            ulong sizeToAlloc = FbxUtils.SizeOf(type);
            var ptr = FbxUtils.FbxMalloc(sizeToAlloc);
            var ptrPtr = (nint)(void*)&ptr;
            GetAtInternal(Handle, index, ptrPtr, type);
            return ptr;
        }

 
    }


    internal class FbxLayerElementArray<T> : FbxNative
        where T : FbxNative, new()
    {
        public FbxLayerElementArray(nint inHandle)
            : base(inHandle)
        {
        }

        public int Count => FbxLayerElementArray.GetCountInternal(Handle);

        private static readonly EFbxType FbxType = FbxUtils.Convert<T>();

        public int Add(T item)
        {
            return FbxLayerElementArray.AddInternal(Handle, 
                item.Handle, FbxType);
        }

        public T? GetAt(int index)
        {
            var res = new T();
            FbxLayerElementArray.GetAtInternal(Handle, index, res.Handle, FbxType);
            return res;
        }
    }
}
