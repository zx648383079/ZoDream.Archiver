using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxNode : FbxObject
    {
        public enum EPivotSet
        {
            eSourcePivot,
            eDestinationPivot
        };

        public enum EShadingMode
        {
            eHardShading,       //!< Solid geometries rendered with smooth surfaces - using the system light.
            eWireFrame,         //!< Geometries displayed in wire frame.
            eFlatShading,       //!< Solid geometries rendered faceted - using the system light.
            eLightShading,      //!< Solid geometries rendered with the scene lights.
            eTextureShading,    //!< Solid geometries rendered with smooth textured surfaces - using system light.
            eFullShading        //!< Solid geometries rendered with smooth textured surfaces and scene lights.
        };

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxNode@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateFromManager(nint pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxNode@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetNodeAttribute@FbxNode@fbxsdk@@QEAAPEAVFbxNodeAttribute@2@PEAV32@@Z")]
        private static extern nint SetNodeAttributeInternal(nint InHandle, nint pNodeAttribute);

        [DllImport(NativeMethods.DllName, EntryPoint = "?AddChild@FbxNode@fbxsdk@@QEAA_NPEAV12@@Z")]
        private static extern bool AddChildInternal(nint InHandle, nint pNode);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetNodeAttribute@FbxNode@fbxsdk@@QEAAPEAVFbxNodeAttribute@2@XZ")]
        private static extern nint GetNodeAttributeInternal(nint inHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetNodeAttributeCount@FbxNode@fbxsdk@@QEBAHXZ")]
        private static extern int GetNodeAttributeCountInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetNodeAttributeByIndex@FbxNode@fbxsdk@@QEAAPEAVFbxNodeAttribute@2@H@Z")]
        private static extern nint GetNodeAttributeByIndexInternal(nint handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?EvaluateGlobalTransform@FbxNode@fbxsdk@@QEAAAEAVFbxAMatrix@2@VFbxTime@2@W4EPivotSet@12@_N2@Z")]
        private static extern nint EvaluateGlobalTransformInternal(nint inHandle, nint pTime, EPivotSet pPivotSet, bool pApplyTarget, bool pForceEval);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetParent@FbxNode@fbxsdk@@QEAAPEAV12@XZ")]
        private static extern nint GetParentInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetChildCount@FbxNode@fbxsdk@@QEBAH_N@Z")]
        private static extern int GetChildCountInternal(nint handle, bool pRecursive);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetChild@FbxNode@fbxsdk@@QEAAPEAV12@H@Z")]
        private static extern nint GetChildInternal(nint handle, int pIndex);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetPreferedAngle@FbxNode@fbxsdk@@QEAAXVFbxVector4@2@@Z")]
        private static extern void SetPreferedAngleInternal(nint handle, nint pPreferedAngle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetMesh@FbxNode@fbxsdk@@QEAAPEAVFbxMesh@2@XZ")]
        private static extern nint GetMeshInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetShadingMode@FbxNode@fbxsdk@@QEAAXW4EShadingMode@12@@Z")]
        private static extern void SetShadingModeInternal(nint pHandle, EShadingMode pShadingMode);
        [DllImport(NativeMethods.DllName, EntryPoint = "?AddMaterial@FbxNode@fbxsdk@@QEAAHPEAVFbxSurfaceMaterial@2@@Z")]
        private static extern int AddMaterialInternal(nint pHandle, nint handle);

        private readonly nint _lclTranslation;
        private readonly nint _lclRotation;
        private readonly nint _lclScaling;
        private readonly nint _visibility;

        public Vector3 LclTranslation { get => FbxProperty.GetDouble3(_lclTranslation); set => FbxProperty.Set(_lclTranslation, value); }
        public Vector3 LclRotation { get => FbxProperty.GetDouble3(_lclRotation); set => FbxProperty.Set(_lclRotation, value); }
        public Vector3 LclScaling { get => FbxProperty.GetDouble3(_lclScaling); set => FbxProperty.Set(_lclScaling, value); }
        public double Visibility { get => FbxProperty.GetDouble(_visibility); set => FbxProperty.Set(_visibility, value); }
        public int ChildCount => GetChildCountInternal(Handle, false);
        public int NodeAttributeCount => GetNodeAttributeCountInternal(Handle);

        public IEnumerable<FbxNode> Children {
            get {
                for (int i = 0; i < ChildCount; i++)
                {
                    var node = GetChild(i);
                    if (node is not null)
                    {
                        yield return node;
                    }
                }
            }
        }

        public FbxNode(FbxManager manager, string pName)
            : this(CreateFromManager(manager.Handle, pName))
        {
        }

        public FbxNode(nint InHandle)
            : base(InHandle)
        {
            _lclTranslation = GetPropertyPtr(0x78);
            _lclRotation = GetPropertyPtr(0x88);
            _lclScaling = GetPropertyPtr(0x98);
            _visibility = GetPropertyPtr(0xA8);
        }

        public FbxNode(FbxObject obj, string pName)
            : this(CreateFromObject(obj.Handle, pName))
        {
        }

        public FbxNodeAttribute? SetNodeAttribute(FbxNodeAttribute pNodeAttribute)
        {
            nint Ptr = SetNodeAttributeInternal(Handle, pNodeAttribute.Handle);
            return Ptr == nint.Zero ? null : new FbxNodeAttribute(Ptr);
        }

        //public FbxNodeAttribute? GetNodeAttribute(FbxNodeAttribute.EType type)
        //{
        //    for (int i = 0; i < NodeAttributeCount; i++)
        //    {
        //        nint ptr = GetNodeAttributeByIndexInternal(Handle, i);
        //        if (ptr != nint.Zero)
        //        {
        //            var attr = new FbxNodeAttribute(ptr);
        //            if (attr.AttributeType == type)
        //            {
        //                return attr;
        //            }
        //        }
        //    }
        //    return null;
        //}

        public bool AddChild(FbxNode pNode)
        {
            return AddChildInternal(Handle, pNode.Handle);
        }

        public FbxAMatrix EvaluateGlobalTransform(FbxTime? time = null, EPivotSet pivotSet = EPivotSet.eSourcePivot, bool applyTarget = false, bool forceEval = false)
        {
            if (time == null)
            {
                time = FbxTime.FBXSDK_TIME_INFINITE;
            }

            nint ptr = EvaluateGlobalTransformInternal(Handle, time.Handle, pivotSet, applyTarget, forceEval);
            return new FbxAMatrix(ptr);
        }

        public FbxNode? GetParent()
        {
            nint ptr = GetParentInternal(Handle);
            return ptr == nint.Zero ? null : new FbxNode(ptr);
        }

        public FbxNode? GetChild(int index)
        {
            nint ptr = GetChildInternal(Handle, index);
            return ptr == nint.Zero ? null : new FbxNode(ptr);
        }

        public void SetPreferedAngle(Vector4 pPreferedAngle)
        {
            var ptr = FbxDouble4.Construct(pPreferedAngle);
            SetPreferedAngleInternal(Handle, ptr);
            FbxUtils.FbxFree(ptr);
        }

        internal FbxMesh GetMesh()
        {
            var ptr = GetMeshInternal(Handle);
            return new FbxMesh(ptr);
        }



        internal FbxAnimCurve LclScalingGetCurve(FbxAnimLayer pAnimLayer, string pChannel, bool pCreate = false)
        {
            return FbxProperty.GetCurve(_lclScaling, pAnimLayer, pChannel, pCreate);
        }

        internal FbxAnimCurve LclRotationGetCurve(FbxAnimLayer pAnimLayer, string pChannel, bool pCreate = false)
        {
            return FbxProperty.GetCurve(_lclRotation, pAnimLayer, pChannel, pCreate);
        }

        internal FbxAnimCurve LclTranslationGetCurve(FbxAnimLayer pAnimLayer, string pChannel, bool pCreate = false)
        {
            return FbxProperty.GetCurve(_lclTranslation, pAnimLayer, pChannel, pCreate);
        }

        internal void SetShadingMode(EShadingMode pShadingMode)
        {
            SetShadingModeInternal(Handle, pShadingMode);
        }

        internal int AddMaterial(FbxSurfacePhong pMat)
        {
            return AddMaterialInternal(Handle, pMat.Handle);
        }

    }
}
