using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxSkeleton : FbxNodeAttribute
    {
        new public enum EType
        {
            eRoot,          /*!< First element of a chain. */
            eLimb,          /*!< Chain element. */
            eLimbNode,      /*!< Chain element. */
            eEffector       /*!< Last element of a chain. */
        };

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSkeleton@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateFromManager(nint pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSkeleton@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetSkeletonType@FbxSkeleton@fbxsdk@@QEAAXW4EType@12@@Z")]
        private static extern void SetSkeletonTypeInternal(nint inHandle, EType pSkeletonType);

        private readonly nint _size;


        public double Size { get => FbxProperty.GetDouble(_size); set => FbxProperty.Set(_size, value); }

        public FbxSkeleton(FbxManager Manager, string pName)
            : this(CreateFromManager(Manager.Handle, pName))
        {
        }

        public FbxSkeleton(nint InHandle)
            : base(InHandle)
        {
            _size = GetPropertyPtr(0x88);
        }

        public FbxSkeleton(FbxObject Object, string pName)
            : this(CreateFromObject(Object.Handle, pName))
        {
        }

        public void SetSkeletonType(EType skeletonType)
        {
            SetSkeletonTypeInternal(Handle, skeletonType);
        }
    }

}
