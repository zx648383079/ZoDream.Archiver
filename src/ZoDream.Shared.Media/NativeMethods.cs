using System.Runtime.InteropServices;

namespace ZoDream.Shared.Media
{
    internal static class NativeMethods
    {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        internal static unsafe extern void MoveMemory(byte* dest, byte* src, int size);
    }
}
