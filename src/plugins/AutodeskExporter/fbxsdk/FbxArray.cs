using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{

    internal class FbxArray
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0?$FbxArray@_N$0BA@@fbxsdk@@QEAA@XZ")]
        internal static extern nint ConstructInternal();
        [DllImport(NativeMethods.DllName, EntryPoint = "??0?$FbxArray@_N$0BA@@fbxsdk@@QEAA@H@Z")]
        internal static extern nint ConstructInternal(int capacity);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetAt@?$FbxArray@_N$0BA@@fbxsdk@@QEBA_NH@Z")]
        internal static extern nint GetAtInternal(nint pHandle, int index);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetAt@?$FbxArray@_N$0BA@@fbxsdk@@QEAAXHAEB_N@Z")]
        internal static extern nint SetAtInternal(nint pHandle, int index, nint data);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Add@?$FbxArray@C$0BA@@fbxsdk@@QEAAHAEBC@Z")]
        internal static extern nint AddInternal(nint pHandle, nint data);
    }

    internal class FbxArray<T> : FbxNative
    {

        public FbxArray(int capacity)
            : this(FbxArray.ConstructInternal(capacity))
        {

        }

        public FbxArray(IntPtr InHandle)
            : base(InHandle)
        {
            _leaveFree = true;
        }

        public T this[int index] {
            get => GetAt(index);
            set => SetAt(index, value);
        }

        private void SetAt(int index, T value)
        {
            if (value is FbxObject obj)
            {
                FbxArray.SetAtInternal(Handle, index, obj.Handle);
                return;
            }
            if (value is Vector4 vec)
            {
                var ptr = FbxDouble4.Construct(vec);
                FbxArray.SetAtInternal(Handle, index, ptr);
                FbxUtils.FbxFree(ptr);
                return;
            }
            throw new NotSupportedException();
        }

        internal void Add(T value)
        {
            if (value is FbxObject obj)
            {
                FbxArray.AddInternal(Handle, obj.Handle);
                return;
            }
            if (value is Vector4 vec)
            {
                var ptr = FbxDouble4.Construct(vec);
                FbxArray.AddInternal(Handle, ptr);
                FbxUtils.FbxFree(ptr);
                return;
            }
            throw new NotSupportedException();
        }

        internal T? GetAt(int index)
        {
            var ptr = FbxArray.GetAtInternal(Handle, index);
            if (ptr == IntPtr.Zero)
            {
                return default;
            }
            var type = typeof(T);
            if (typeof(FbxObject).IsAssignableTo(type))
            {
                return (T)Activator.CreateInstance(type, ptr);
            }
            if (type == typeof(Vector4))
            {
                return (T)(object)FbxDouble4.Get(ptr);
            }
            return default;
        }

    }

}
