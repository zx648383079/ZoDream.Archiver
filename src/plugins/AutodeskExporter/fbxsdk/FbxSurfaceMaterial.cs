using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxSurfaceMaterial : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSurfaceMaterial@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);


        private readonly nint _shadingModel;

        public string ShadingModel { get => FbxProperty.GetString(_shadingModel); set => FbxProperty.Set(_shadingModel, value); }

        public FbxSurfaceMaterial(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {

        }

        public FbxSurfaceMaterial(nint handle)
            : base(handle)
        {
            _shadingModel = GetPropertyPtr(0x78);
        }
    }

}
