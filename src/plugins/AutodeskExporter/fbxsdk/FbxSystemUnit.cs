using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxSystemUnit : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetScaleFactor@FbxSystemUnit@fbxsdk@@QEBANXZ")]
        private static extern double GetScaleFactorInternal(IntPtr InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "??1FbxSystemUnit@fbxsdk@@QEAA@XZ")]
        private static extern IntPtr CreateInternal(double pScaleFactor, double pMultiplier);

        private static FbxSystemUnit? mMillimeters;
        private static FbxSystemUnit? mCentimeters;
        private static FbxSystemUnit? mMeters;
        private static FbxSystemUnit? mKilometers;

        public FbxSystemUnit() { }

        public FbxSystemUnit(double pScaleFactor, double pMultiplier = 1.0)
            : base(CreateInternal(pScaleFactor, pMultiplier))
        {

        }
        public FbxSystemUnit(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public double ScaleFactor => GetScaleFactorInternal(pHandle);

        public static FbxSystemUnit Millimeters => GetStaticValue("?mm@FbxSystemUnit@fbxsdk@@2V12@B", ref mMillimeters);
        public static FbxSystemUnit Centimeters => GetStaticValue("?cm@FbxSystemUnit@fbxsdk@@2V12@B", ref mCentimeters);
        public static FbxSystemUnit Meters => GetStaticValue("?m@FbxSystemUnit@fbxsdk@@2V12@B", ref mMeters);
        public static FbxSystemUnit Kilometers => GetStaticValue("?km@FbxSystemUnit@fbxsdk@@2V12@B", ref mKilometers);

        private static FbxSystemUnit GetStaticValue(string Sig, ref FbxSystemUnit? OutUnit)
        {
            if (OutUnit == null)
            {
                IntPtr Module = NativeMethods.LoadLibrary(NativeMethods.DllFullPath);
                OutUnit = new FbxSystemUnit(NativeMethods.GetProcAddress(Module, Sig));
                NativeMethods.FreeLibrary(Module);
            }
            return OutUnit;
        }
    }
}
