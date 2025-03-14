using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxSurfaceLambert : FbxSurfaceMaterial
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSurfaceLambert@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);


        private readonly nint _diffuse;
        private readonly nint _ambient;
        private readonly nint _emissive;
        private readonly nint _transparencyFactor;
        private readonly nint _normalMap;
        private readonly nint _bump;


        public Vector3 Diffuse { get => FbxProperty.GetDouble3(_diffuse); set => FbxProperty.Set(_diffuse, value); }
        public Vector3 Ambient { get => FbxProperty.GetDouble3(_ambient); set => FbxProperty.Set(_ambient, value); }
        public Vector3 Emissive { get => FbxProperty.GetDouble3(_emissive); set => FbxProperty.Set(_emissive, value); }
        public double TransparencyFactor { get => FbxProperty.GetDouble(_transparencyFactor); set => FbxProperty.Set(_transparencyFactor, value); }
        
        public FbxSurfaceLambert(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }

        public FbxSurfaceLambert(IntPtr handle)
            : base(handle)
        {
            _diffuse = handle + 0xd8;
            _ambient = handle + 0xb8;
            _emissive = handle + 0x98;
            _transparencyFactor = handle + 0x138;
            _normalMap = handle + 0xF8;
            _bump = handle + 0x108;
        }

        internal void BumpConnectSrcObject(FbxFileTexture pTexture)
        {
            FbxProperty.ConnectSrcObject(_bump, pTexture);
        }

        internal void DiffuseConnectSrcObject(FbxFileTexture pTexture)
        {
            FbxProperty.ConnectSrcObject(_diffuse, pTexture);
        }

        internal void NormalMapConnectSrcObject(FbxFileTexture pTexture)
        {
            FbxProperty.ConnectSrcObject(_normalMap, pTexture);
        }

    
    }

}
