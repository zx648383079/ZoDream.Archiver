using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxGlobalSettings : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetSystemUnit@FbxGlobalSettings@fbxsdk@@QEAAXAEBVFbxSystemUnit@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetSystemUnitInternal(IntPtr InHandle, IntPtr pOther);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetTimeMode@FbxGlobalSettings@fbxsdk@@QEAAXW4EMode@FbxTime@2@@Z")]
        private static extern void SetTimeModeInternal(IntPtr InHandle, EMode mode);

        public FbxGlobalSettings() { }
        public FbxGlobalSettings(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public void SetSystemUnit(FbxSystemUnit pOther)
        {
            IntPtr Ptr = pOther.Handle;
            SetSystemUnitInternal(Handle, Ptr);
        }

        public void SetTimeMode(EMode mode)
        {
            SetTimeModeInternal(Handle, mode);
        }
    }

}
