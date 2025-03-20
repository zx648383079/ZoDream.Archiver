using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxSurfacePhong : FbxSurfaceLambert
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSurfacePhong@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint handle, string name);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSurfacePhong@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateFromManager(nint handle, string name);

        private readonly nint _specular;
        private readonly nint _reflection;
        private readonly nint _shininess;
        public Vector3 Specular { get => FbxProperty.GetDouble3(_specular); set => FbxProperty.Set(_specular, value); }
        public Vector3 Reflection { get => FbxProperty.GetDouble3(_reflection); set => FbxProperty.Set(_reflection, value); }
        public double Shininess { get => FbxProperty.GetDouble(_shininess); set => FbxProperty.Set(_shininess, value); }

        public FbxSurfacePhong(FbxManager manager, string name)
            : this(CreateFromManager(manager.Handle, name))
        {
            
        }

        public FbxSurfacePhong(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }

        public FbxSurfacePhong(nint handle)
            : base(handle)
        {
            _specular = GetPropertyPtr(0x188);
            _reflection = GetPropertyPtr(0x1B8);
            _shininess = GetPropertyPtr(0x1A8);
        }

        internal void SpecularConnectSrcObject(FbxFileTexture pTexture)
        {
            new FbxProperty(_specular).Object.ConnectSrcObject(pTexture);
        }
    }

}
