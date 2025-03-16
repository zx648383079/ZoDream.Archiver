using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxObject : FbxNative, IDisposable
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Destroy@FbxObject@fbxsdk@@QEAAX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        protected static extern nint DestroyInternal(nint InHandle, bool pRecursive);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetName@FbxObject@fbxsdk@@QEBAPEBDXZ", CallingConvention = CallingConvention.ThisCall)]
        protected static extern nint GetNameInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetName@FbxObject@fbxsdk@@QEAAXPEBD@Z", CallingConvention = CallingConvention.ThisCall)]
        protected static extern void SetNameInternal(nint InHandle, [MarshalAs(UnmanagedType.LPStr)] string pName);

        internal FbxObject()
        {
        }

        public FbxObject(nint InHandle)
            : base(InHandle)
        {
        }

        public string Name {
            get {
                nint StrPtr = GetNameInternal(Handle);
                return FbxUtils.IntPtrToString(StrPtr);
            }
            set => SetNameInternal(Handle, value);
        }

        protected override void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                DestroyInternal(Handle, false);
            }
            // base.Dispose(bDisposing);
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is FbxObject b)
            {
                return Handle == b.Handle;
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
            {
                return !ReferenceEquals(b, null);
            }

            return !a.Equals(b);
        }
    }

}
