using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxObject : FbxNative, IDisposable
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Destroy@FbxObject@fbxsdk@@QEAAX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        protected static extern IntPtr DestroyInternal(IntPtr InHandle, bool pRecursive);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetName@FbxObject@fbxsdk@@QEBAPEBDXZ", CallingConvention = CallingConvention.ThisCall)]
        protected static extern IntPtr GetNameInternal(IntPtr InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetName@FbxObject@fbxsdk@@QEAAXPEBD@Z", CallingConvention = CallingConvention.ThisCall)]
        protected static extern void SetNameInternal(IntPtr InHandle, [MarshalAs(UnmanagedType.LPStr)] string pName);

        internal FbxObject()
        {
        }

        public FbxObject(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public string Name {
            get {
                IntPtr StrPtr = GetNameInternal(Handle);
                return FbxUtils.IntPtrToString(StrPtr);
            }
            set => SetNameInternal(Handle, value);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool bDisposing)
        {
            if (pHandle != IntPtr.Zero)
            {
                DestroyInternal(pHandle, false);
                pHandle = IntPtr.Zero;
            }

            if (bDisposing)
                GC.SuppressFinalize(this);
        }

        public override int GetHashCode()
        {
            return pHandle.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is FbxObject b)
            {
                return pHandle == b.Handle;
            }
            return false;
        }

        public static bool operator ==(FbxObject a, FbxObject b)
        {
            return a?.Equals(b) ?? b is null;
        }

        public static bool operator !=(FbxObject a, FbxObject b)
        {
            if (ReferenceEquals(a, null))
                return !ReferenceEquals(b, null);

            return !a.Equals(b);
        }
    }

}
