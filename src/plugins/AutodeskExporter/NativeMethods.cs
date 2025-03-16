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

        static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == DllName)
            {
                return NativeLibrary.Load(DllFullPath, assembly, searchPath);
            }
            return nint.Zero;
        }

        internal static void Ready() 
        {
            
        }

        internal static string DllFullPath => Path.Combine(AppContext.BaseDirectory,
                    RuntimeInformation.ProcessArchitecture switch
                    {
                        Architecture.X64 or Architecture.Arm64 =>
                            Enum.GetName(RuntimeInformation.ProcessArchitecture).ToLower(),
                        _ => throw new NotImplementedException(),
                    },
                    DllName + ".dll");
    }
}
