using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal static partial class NativeMethods
    {
        internal const string DllName = "libfbxsdk";

        static NativeMethods()
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, DllImportResolver);
        }

        static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == DllName)
            {
                var entry = Path.Combine(AppContext.BaseDirectory,
                    RuntimeInformation.ProcessArchitecture switch
                    {
                        Architecture.X64 or Architecture.Arm64 =>
                            Enum.GetName(RuntimeInformation.ProcessArchitecture).ToLower(),
                        _ => throw new NotImplementedException(),
                    },
                    DllName + ".dll");
                return NativeLibrary.Load(entry, assembly, searchPath);
            }
            return IntPtr.Zero;
        }
    }
}
