using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.AutodeskExporter
{
    internal static partial class NativeMethods
    {
        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string lpszLib);
        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary", SetLastError = true)]
        internal static extern IntPtr FreeLibrary(IntPtr hModule);
    }
}
