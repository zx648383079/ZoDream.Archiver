using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerElementArray : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Add@FbxLayerElementArray@fbxsdk@@QEAAHPEBXW4EFbxType@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern int AddInternal(IntPtr InHandle, IntPtr pItem, EFbxType pValueType);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetCount@FbxLayerElementArray@fbxsdk@@QEBAHXZ")]
        private static extern int GetCountInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetAt@FbxLayerElementArray@fbxsdk@@QEBA_NHPEAPEAXW4EFbxType@2@@Z")]
        private static extern bool GetAtInternal(IntPtr handle, int pIndex, IntPtr pItem, EFbxType pValueType);

        public int Count => GetCountInternal(Handle);

        public FbxLayerElementArray(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public int Add(double x, double y, double z, double w = 0.0)
        {
            IntPtr ptr = FbxUtils.FbxMalloc(32);

            Marshal.WriteInt64(ptr, 0, BitConverter.ToInt64(BitConverter.GetBytes(x), 0));
            Marshal.WriteInt64(ptr, 8, BitConverter.ToInt64(BitConverter.GetBytes(y), 0));
            Marshal.WriteInt64(ptr, 16, BitConverter.ToInt64(BitConverter.GetBytes(z), 0));
            Marshal.WriteInt64(ptr, 24, BitConverter.ToInt64(BitConverter.GetBytes(w), 0));

            int idx = AddInternal(Handle, ptr, EFbxType.eFbxDouble4);
            FbxUtils.FbxFree(ptr);

            return idx;
        }

        public int Add(double x, double y)
        {
            IntPtr ptr = FbxUtils.FbxMalloc(16);

            Marshal.WriteInt64(ptr, 0, BitConverter.ToInt64(BitConverter.GetBytes(x), 0));
            Marshal.WriteInt64(ptr, 8, BitConverter.ToInt64(BitConverter.GetBytes(y), 0));

            int idx = AddInternal(Handle, ptr, EFbxType.eFbxDouble2);
            FbxUtils.FbxFree(ptr);

            return idx;
        }

        public int Add(int a)
        {
            IntPtr ptr = FbxUtils.FbxMalloc(4);
            Marshal.WriteInt32(ptr, 0, a);

            int idx = AddInternal(Handle, ptr, EFbxType.eFbxInt);
            FbxUtils.FbxFree(ptr);

            return idx;
        }

        public void GetAt(int index, out Vector4 outValue)
        {
            outValue = new Vector4();
            IntPtr ptr = GetAt(index, EFbxType.eFbxDouble4);

            outValue.X = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 0));
            outValue.Y = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 8));
            outValue.Z = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 16));
            outValue.W = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 24));
            FbxUtils.FbxFree(ptr);
        }

        public void GetAt(int index, out Vector3 outValue)
        {
            outValue = new Vector3();
            IntPtr ptr = GetAt(index, EFbxType.eFbxDouble3);

            outValue.X = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 0));
            outValue.Y = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 8));
            outValue.Z = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 16));
            FbxUtils.FbxFree(ptr);
        }

        public void GetAt(int index, out Vector2 outValue)
        {
            outValue = new Vector2();
            IntPtr ptr = GetAt(index, EFbxType.eFbxDouble2);

            outValue.X = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 0));
            outValue.Y = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr, 8));
            FbxUtils.FbxFree(ptr);
        }

        public void GetAt(int index, out int outValue)
        {
            IntPtr ptr = GetAt(index, EFbxType.eFbxInt);
            outValue = Marshal.ReadInt32(ptr);
            FbxUtils.FbxFree(ptr);
        }

        private unsafe IntPtr GetAt(int index, EFbxType type)
        {
            ulong sizeToAlloc = 0;
            switch (type)
            {
                case EFbxType.eFbxDouble4: sizeToAlloc = 8 * 4; break;
                case EFbxType.eFbxDouble3: sizeToAlloc = 8 * 3; break;
                case EFbxType.eFbxDouble2: sizeToAlloc = 8 * 2; break;
                case EFbxType.eFbxInt: sizeToAlloc = 4; break;
            }

            IntPtr ptr = FbxUtils.FbxMalloc(sizeToAlloc);
            IntPtr ptrPtr = new IntPtr((void*)&ptr);
            GetAtInternal(Handle, index, ptrPtr, type);
            ptrPtr = IntPtr.Zero;

            return ptr;
        }
    }

}
