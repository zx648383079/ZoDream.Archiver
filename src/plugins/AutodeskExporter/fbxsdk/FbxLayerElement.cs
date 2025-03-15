using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerElement : FbxNative
    {
        internal enum EType
        {
            eUnknown,
            eNormal,
            eBiNormal,
            eTangent,
            eMaterial,
            ePolygonGroup,
            eUV,
            eVertexColor,
            eSmoothing,
            eVertexCrease,
            eEdgeCrease,
            eHole,
            eUserData,
            eVisibility,
            eTextureDiffuse,
            eTextureDiffuseFactor,
            eTextureEmissive,
            eTextureEmissiveFactor,
            eTextureAmbient,
            eTextureAmbientFactor,
            eTextureSpecular,
            eTextureSpecularFactor,
            eTextureShininess,
            eTextureNormalMap,
            eTextureBump,
            eTextureTransparency,
            eTextureTransparencyFactor,
            eTextureReflection,
            eTextureReflectionFactor,
            eTextureDisplacement,
            eTextureDisplacementVector,
            eTypeCount
        };

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetMappingMode@FbxLayerElement@fbxsdk@@QEAAXW4EMappingMode@12@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetMappingModeInternal(IntPtr InHandle, EMappingMode pMappingMode);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetReferenceMode@FbxLayerElement@fbxsdk@@QEAAXW4EReferenceMode@12@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetReferenceModeInternal(IntPtr InHandle, EReferenceMode pReferenceMode);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetMappingMode@FbxLayerElement@fbxsdk@@QEBA?AW4EMappingMode@12@XZ")]
        private static extern EMappingMode GetMappingModeInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetReferenceMode@FbxLayerElement@fbxsdk@@QEBA?AW4EReferenceMode@12@XZ")]
        private static extern EReferenceMode GetReferenceModeInternal(IntPtr handle);

        public EMappingMode MappingMode { get => GetMappingModeInternal(Handle); set => SetMappingModeInternal(Handle, value); }
        public EReferenceMode ReferenceMode { get => GetReferenceModeInternal(Handle); set => SetReferenceModeInternal(Handle, value); }
        public string Name { get => FbxString.Get(_name); set => _name = FbxString.Construct(value); }

        private IntPtr _name;

        private FbxLayerElement()
        {
        }

        public FbxLayerElement(IntPtr handle)
            : base(handle)
        {
            _name = GetPropertyPtr(0x10);
        }
    }

}
