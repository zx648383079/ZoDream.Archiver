using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxIOSettings : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxIOSettings@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateInternal(nint pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetBoolProp@FbxIOSettings@fbxsdk@@QEAAXPEBD_N@Z")]
        private static extern void SetBoolPropInternal(nint InHandle, [MarshalAs(UnmanagedType.LPStr)] string pName, bool pValue);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetBoolProp@FbxIOSettings@fbxsdk@@QEBA_NPEBD_N@Z")]
        private static extern bool GetBoolPropInternal(nint InHandle, [MarshalAs(UnmanagedType.LPStr)] string pName);

        #region -- Strings --

        public const string IOSROOT = "IOSRoot";
        public const string IOSN_EXPORT = "Export";
        public const string IOSN_ADV_OPT_GRP = "AdvOptGrp";
        public const string IOSN_FBX = "Fbx";
        public const string IOSN_MATERIAL = "Material";
        public const string IOSN_TEXTURE = "Texture";
        public const string IOSN_GLOBAL_SETTINGS = "Global_Settings";
        public const string IOSN_SHAPE = "Shape";
        public const string IOSN_EMBEDDED = "EMBEDDED";
        public const string IOSN_GOBO = "Gobo";
        public const string IOSN_ANIMATION = "Animation";
        public const string EXP_ADV_OPT_GRP = IOSN_EXPORT + "|" + IOSN_ADV_OPT_GRP;
        public const string EXP_FBX = EXP_ADV_OPT_GRP + "|" + IOSN_FBX;
        public const string EXP_FBX_MATERIAL = EXP_FBX + "|" + IOSN_MATERIAL;
        public const string EXP_FBX_TEXTURE = EXP_FBX + "|" + IOSN_TEXTURE;
        public const string EXP_FBX_GLOBAL_SETTINGS = EXP_FBX + "|" + IOSN_GLOBAL_SETTINGS;
        public const string EXP_FBX_SHAPE = EXP_FBX + "|" + IOSN_SHAPE;
        public const string EXP_FBX_EMBEDDED = EXP_FBX + "|" + IOSN_EMBEDDED;
        public const string EXP_FBX_GOBO = EXP_FBX + "|" + IOSN_GOBO;
        public const string EXP_FBX_ANIMATION = EXP_FBX + "|" + IOSN_ANIMATION;

        #endregion

        public FbxIOSettings() { }
        public FbxIOSettings(nint InHandle)
            : base(InHandle)
        {
        }

        public FbxIOSettings(FbxManager Manager, string Name)
            : base(CreateInternal(Manager.Handle, Name))
        {
        }

        public void SetBoolProp(string pName, bool pValue)
        {
            SetBoolPropInternal(Handle, pName, pValue);
        }

        public bool GetBoolProp(string pName)
        {
            return GetBoolPropInternal(Handle, pName);
        }
    }

}
