using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxStream : FbxNative
    {
        internal enum EState
        {
            eClosed,    //!< The stream is closed.
            eOpen,      //!< The stream is open.
            eEmpty      //!< The stream is empty.
        };

        internal enum ESeekPos { eBegin, eCurrent, eEnd };

        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxStream@fbxsdk@@QEAA@XZ")]
        internal static extern void ConstructInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "???")]
        internal static extern void OpenInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "???")]
        internal static extern void CloseInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "???")]
        internal static extern void FlushInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "???")]
        internal static extern void SeekInternal(nint pHandle, ref long pOffset, ref ESeekPos eSeek);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Read@FbxStream@fbxsdk@@QEBA_KPEAD_K@Z")]
        internal static extern long ReadInternal(nint pHandle, nint buffer, ulong pSize);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Write@FbxStream@fbxsdk@@QEAA_KPEBD_K@Z")]
        internal static extern long WriteInternal(nint pHandle, nint buffer, ulong pSize);

        [DllImport(NativeMethods.DllName, EntryPoint = "???")]
        internal static extern long GetPositionInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "???")]
        internal static extern void SetPositionInternal(nint pHandle, long pPosition);

        [DllImport(NativeMethods.DllName, EntryPoint = "???")]
        internal static extern int GetStateInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "???")]
        internal static extern int GetErrorInternal(nint pHandle);

        public void Flush()
        {

        }

        public void Close()
        {

        }
    }
}
