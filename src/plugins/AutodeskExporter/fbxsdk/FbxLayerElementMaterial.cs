using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerElementMaterial : FbxLayerElement
    {

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxLayerElementMaterial@fbxsdk@@SAPEAV12@PEAVFbxLayerContainer@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pOwner, [MarshalAs(UnmanagedType.LPStr)] string pName);

        public FbxLayerElementMaterial(nint handle)
            : base(handle)
        {
        }

        public FbxLayerElementMaterial(FbxLayerContainer container, string name)
            : base(CreateFromObject(container.Handle, name))
        {
            
        }

        public void AddDirect(FbxSurfaceMaterial data)
        {
        }

    }
}
