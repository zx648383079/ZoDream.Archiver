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
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxNode@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetNodeAttribute@FbxNode@fbxsdk@@QEAAPEAVFbxNodeAttribute@2@PEAV32@@Z")]
        private static extern IntPtr SetNodeAttributeInternal(IntPtr InHandle, IntPtr pNodeAttribute);

        [DllImport(NativeMethods.DllName, EntryPoint = "?AddChild@FbxNode@fbxsdk@@QEAA_NPEAV12@@Z")]
        private static extern bool AddChildInternal(IntPtr InHandle, IntPtr pNode);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetNodeAttribute@FbxNode@fbxsdk@@QEAAPEAVFbxNodeAttribute@2@XZ")]
        private static extern IntPtr GetNodeAttributeInternal(IntPtr inHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetNodeAttributeCount@FbxNode@fbxsdk@@QEBAHXZ")]
        private static extern int GetNodeAttributeCountInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetNodeAttributeByIndex@FbxNode@fbxsdk@@QEAAPEAVFbxNodeAttribute@2@H@Z")]
        private static extern IntPtr GetNodeAttributeByIndexInternal(IntPtr handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?EvaluateGlobalTransform@FbxNode@fbxsdk@@QEAAAEAVFbxAMatrix@2@VFbxTime@2@W4EPivotSet@12@_N2@Z")]
        private static extern IntPtr EvaluateGlobalTransformInternal(IntPtr inHandle, IntPtr pTime, EPivotSet pPivotSet, bool pApplyTarget, bool pForceEval);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetParent@FbxNode@fbxsdk@@QEAAPEAV12@XZ")]
        private static extern IntPtr GetParentInternal(IntPtr pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetChildCount@FbxNode@fbxsdk@@QEBAH_N@Z")]
        private static extern int GetChildCountInternal(IntPtr handle, bool pRecursive);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetChild@FbxNode@fbxsdk@@QEAAPEAV12@H@Z")]
        private static extern IntPtr GetChildInternal(IntPtr handle, int pIndex);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetPreferedAngle@FbxNode@fbxsdk@@QEAAXVFbxVector4@2@@Z")]
        private static extern void SetPreferedAngleInternal(IntPtr handle, IntPtr pPreferedAngle);

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
        public int ChildCount => GetChildCountInternal(pHandle, false);
        public int NodeAttributeCount => GetNodeAttributeCountInternal(pHandle);

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

        public FbxNode(FbxManager Manager, string pName)
        {
            pHandle = CreateFromManager(Manager.Handle, pName);

            _lclTranslation = pHandle + 0x78;
            _lclRotation = pHandle + 0x88;
            _lclScaling = pHandle + 0x98;
            _visibility = pHandle + 0xA8;
        }

        public FbxNode(IntPtr InHandle)
            : base(InHandle)
        {
            _lclTranslation = pHandle + 0x78;
            _lclRotation = pHandle + 0x88;
            _lclScaling = pHandle + 0x98;
            _visibility = pHandle + 0xA8;
        }

        public FbxNode(FbxObject Object, string pName)
        {
            pHandle = CreateFromObject(Object.Handle, pName);

            _lclTranslation = pHandle + 0x78;
            _lclRotation = pHandle + 0x88;
            _lclScaling = pHandle + 0x98;
            _visibility = pHandle + 0xA8;
        }

        public FbxNodeAttribute? SetNodeAttribute(FbxNodeAttribute pNodeAttribute)
        {
            IntPtr Ptr = SetNodeAttributeInternal(pHandle, pNodeAttribute.Handle);
            return Ptr == IntPtr.Zero ? null : new FbxNodeAttribute(Ptr);
        }

        public FbxNodeAttribute? GetNodeAttribute(FbxNodeAttribute.EType type)
        {
            for (int i = 0; i < NodeAttributeCount; i++)
            {
                IntPtr ptr = GetNodeAttributeByIndexInternal(pHandle, i);
                if (ptr != IntPtr.Zero)
                {
                    var attr = new FbxNodeAttribute(ptr);
                    if (attr.AttributeType == type)
                    {
                        return attr;
                    }
                }
            }
            return null;
        }

        public bool AddChild(FbxNode pNode)
        {
            return AddChildInternal(pHandle, pNode.Handle);
        }

        public FbxAMatrix EvaluateGlobalTransform(FbxTime? time = null, EPivotSet pivotSet = EPivotSet.eSourcePivot, bool applyTarget = false, bool forceEval = false)
        {
            if (time == null)
            {
                time = FbxTime.FBXSDK_TIME_INFINITE;
            }

            IntPtr ptr = EvaluateGlobalTransformInternal(pHandle, time.Handle, pivotSet, applyTarget, forceEval);
            return new FbxAMatrix(ptr);
        }

        public FbxNode? GetParent()
        {
            IntPtr ptr = GetParentInternal(pHandle);
            return ptr == IntPtr.Zero ? null : new FbxNode(ptr);
        }

        public FbxNode? GetChild(int index)
        {
            IntPtr ptr = GetChildInternal(pHandle, index);
            return ptr == IntPtr.Zero ? null : new FbxNode(ptr);
        }

        public void SetPreferedAngle(Vector4 pPreferedAngle)
        {
            IntPtr ptr = FbxDouble4.Construct(pPreferedAngle);
            SetPreferedAngleInternal(pHandle, ptr);
            FbxUtils.FbxFree(ptr);
        }

        internal FbxMesh GetMesh()
        {
            var ptr = GetMeshInternal(pHandle);
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
            SetShadingModeInternal(pHandle, pShadingMode);
        }

        internal int AddMaterial(FbxSurfacePhong pMat)
        {
            return AddMaterialInternal(pHandle, pMat.Handle);
        }

    }
}
