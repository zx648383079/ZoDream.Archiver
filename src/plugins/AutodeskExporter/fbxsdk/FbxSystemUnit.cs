using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxSystemUnit : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetScaleFactor@FbxSystemUnit@fbxsdk@@QEBANXZ")]
        private static extern double GetScaleFactorInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxSystemUnit@fbxsdk@@QEAA@NN@Z")]
        private static extern void ConstructInternal(nint pHandle, double pScaleFactor, double pMultiplier);

        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxSystemUnit)
        /// </summary>
        const ulong SizeOfThis = 0x10;

        private static FbxSystemUnit? mMillimeters;
        private static FbxSystemUnit? mCentimeters;
        private static FbxSystemUnit? mMeters;
        private static FbxSystemUnit? mKilometers;

        public FbxSystemUnit() { }

        public FbxSystemUnit(double pScaleFactor, double pMultiplier = 1.0)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle, pScaleFactor, pMultiplier);
            _leaveFree = true;
        }
        public FbxSystemUnit(nint InHandle)
            : base(InHandle)
        {
        }

        public double ScaleFactor => GetScaleFactorInternal(Handle);

        public static FbxSystemUnit Millimeters => GetStaticValue("?mm@FbxSystemUnit@fbxsdk@@2V12@B", ref mMillimeters);
        public static FbxSystemUnit Centimeters => GetStaticValue("?cm@FbxSystemUnit@fbxsdk@@2V12@B", ref mCentimeters);
        public static FbxSystemUnit Meters => GetStaticValue("?m@FbxSystemUnit@fbxsdk@@2V12@B", ref mMeters);
        public static FbxSystemUnit Kilometers => GetStaticValue("?km@FbxSystemUnit@fbxsdk@@2V12@B", ref mKilometers);

        private static FbxSystemUnit GetStaticValue(string Sig, ref FbxSystemUnit? OutUnit)
        {
            if (OutUnit == null)
            {
                nint Module = NativeMethods.LoadLibrary(NativeMethods.DllFullPath);
                OutUnit = new FbxSystemUnit(NativeMethods.GetProcAddress(Module, Sig));
                NativeMethods.FreeLibrary(Module);
            }
            return OutUnit;
        }
    }
}
