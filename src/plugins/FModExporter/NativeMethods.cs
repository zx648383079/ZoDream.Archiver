using FMOD;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZoDream.FModExporter
{
    internal static partial class NativeMethods
    {
        static NativeMethods()
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, DllImportResolver);
        }

        static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == VERSION.dll)
            {
                var entry = Path.Combine(AppContext.BaseDirectory,
                    RuntimeInformation.ProcessArchitecture switch
                    {
                        Architecture.X86 or Architecture.X64 or Architecture.Arm64 => 
                            Enum.GetName(RuntimeInformation.ProcessArchitecture).ToLower(),
                        _ => throw new NotImplementedException(),
                    },
                    VERSION.dll + ".dll");
                return NativeLibrary.Load(entry, assembly, searchPath);
            }

            return IntPtr.Zero;
        }

        public static void Ready()
        {

        }
    }
}
