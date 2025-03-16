using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxAnimCurve : FbxCollection
    {
        internal enum ETangentMode
        {
            eTangentAuto = 0x00000100,                                                  //!< Auto key (spline cardinal).
            eTangentTCB = 0x00000200,                                                   //!< Spline TCB (Tension, Continuity, Bias)
            eTangentUser = 0x00000400,                                                  //!< Next slope at the left equal to slope at the right.
            eTangentGenericBreak = 0x00000800,                                          //!< Independent left and right slopes.
            eTangentBreak = eTangentGenericBreak | eTangentUser,                            //!< Independent left and right slopes, with next slope at the left equal to slope at the right.
            eTangentAutoBreak = eTangentGenericBreak | eTangentAuto,                        //!< Independent left and right slopes, with auto key.
            eTangentGenericClamp = 0x00001000,                                          //!< Clamp: key should be flat if next or previous key has the same value (overrides tangent mode).
            eTangentGenericTimeIndependent = 0x00002000,                                //!< Time independent tangent (overrides tangent mode).
            eTangentGenericClampProgressive = 0x00004000 | eTangentGenericTimeIndependent   //!< Clamp progressive: key should be flat if tangent control point is outside [next-previous key] range (overrides tangent mode).
        };

        //! Key interpolation type.
        internal enum EInterpolationType
        {
            eInterpolationConstant = 0x00000002,    //!< Constant value until next key.
            eInterpolationLinear = 0x00000004,      //!< Linear progression to next key.
            eInterpolationCubic = 0x00000008        //!< Cubic progression to next key.
        };

        //! Weighted mode.
        internal enum EWeightedMode
        {
            eWeightedNone = 0x00000000,                     //!< Tangent has default weights of 0.333; we define this state as not weighted.
            eWeightedRight = 0x01000000,                    //!< Right tangent is weighted.
            eWeightedNextLeft = 0x02000000,                 //!< Left tangent is weighted.
            eWeightedAll = eWeightedRight | eWeightedNextLeft   //!< Both left and right tangents are weighted.
        };

        //! Key constant mode.
        internal enum EConstantMode
        {
            eConstantStandard = 0x00000000, //!< Curve value is constant between this key and the next
            eConstantNext = 0x00000100      //!< Curve value is constant, with next key's value
        };

        //! Velocity mode. Velocity settings speed up or slow down animation on either side of a key without changing the trajectory of the animation. Unlike Auto and Weight settings, Velocity changes the animation in time, but not in space.
        internal enum EVelocityMode
        {
            eVelocityNone = 0x00000000,                     //!< No velocity (default).
            eVelocityRight = 0x10000000,                    //!< Right tangent has velocity.
            eVelocityNextLeft = 0x20000000,                 //!< Left tangent has velocity.
            eVelocityAll = eVelocityRight | eVelocityNextLeft   //!< Both left and right tangents have velocity.
        };

        //! Tangent visibility.
        internal enum ETangentVisibility
        {
            eTangentShowNone = 0x00000000,                          //!< No tangent is visible.
            eTangentShowLeft = 0x00100000,                          //!< Left tangent is visible.
            eTangentShowRight = 0x00200000,                         //!< Right tangent is visible.
            eTangentShowBoth = eTangentShowLeft | eTangentShowRight //!< Both left and right tangents are visible.
        };

        //! FbxAnimCurveKey data indices for cubic interpolation tangent information.
        internal enum EDataIndex
        {
            eRightSlope = 0,        //!< Index of the right derivative, User and Break tangent mode (data are float).
            eNextLeftSlope = 1,     //!< Index of the left derivative for the next key, User and Break tangent mode.
            eWeights = 2,           //!< Start index of weight values, User and Break tangent break mode (data are FbxInt16 tokens from weight and converted to float).
            eRightWeight = 2,       //!< Index of weight on right tangent, User and Break tangent break mode.
            eNextLeftWeight = 3,    //!< Index of weight on next key's left tangent, User and Break tangent break mode.
            eVelocity = 4,          //!< Start index of velocity values, Velocity mode
            eRightVelocity = 4,     //!< Index of velocity on right tangent, Velocity mode
            eNextLeftVelocity = 5,  //!< Index of velocity on next key's left tangent, Velocity mode
            eTCBTension = 0,        //!< Index of Tension, TCB tangent mode (data are floats).
            eTCBContinuity = 1,     //!< Index of Continuity, TCB tangent mode.
            eTCBBias = 2            //!< Index of Bias, TCB tangent mode.
        };

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxAnimCurve@fbxsdk@@SAPEAV12@PEAVFbxScene@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        


        [DllImport(NativeMethods.DllName, EntryPoint = "?KeyModifyBegin@FbxAnimCurveKFCurve@fbxsdk@@UEAAXXZ")]
        private static extern void KeyModifyBeginInternal(nint pObject);
        [DllImport(NativeMethods.DllName, EntryPoint = "?KeySet@FbxAnimCurveKFCurve@fbxsdk@@UEAAXHVFbxTime@2@MW4EInterpolationType@FbxAnimCurveDef@2@W4ETangentMode@52@MMW4EWeightedMode@52@MMMM@Z")]
        private static extern void KeySetInternal(nint pObject, nint lTime, float pValue);
        [DllImport(NativeMethods.DllName, EntryPoint = "?KeyAdd@FbxAnimCurveKFCurve@fbxsdk@@UEAAHVFbxTime@2@PEAH@Z")]
        private static extern int KeyAddInternal(nint pObject, nint lTime);

        [DllImport(NativeMethods.DllName, EntryPoint = "?KeyModifyEnd@FbxAnimCurveKFCurve@fbxsdk@@UEAAXXZ")]
        private static extern void KeyModifyEndInternal(nint pObject);
        [DllImport(NativeMethods.DllName, EntryPoint = "?KeySetValue@FbxAnimCurveKFCurve@fbxsdk@@UEAAXHM@Z")]
        private static extern void KeySetValueInternal(nint pObject, int pKeyIndex, float value);

        [DllImport(NativeMethods.DllName, EntryPoint = "?KeySetInterpolation@FbxAnimCurveKFCurve@fbxsdk@@UEAAXHW4EInterpolationType@FbxAnimCurveDef@2@@Z")]
        private static extern void KeySetInterpolationInternal(nint pObject, int pKeyIndex, EInterpolationType interpolationType);
      

        internal void KeyModifyBegin()
        {
            KeyModifyBeginInternal(Handle);
        }

        internal void KeySet(int pKeyIndex, FbxTime lTime, float pValue)
        {
            KeySetInternal(Handle, lTime.Handle, pValue);
        }

        internal int KeyAdd(FbxTime lTime)
        {
            return KeyAddInternal(Handle, lTime.Handle);
        }

        internal void KeyModifyEnd()
        {
            KeyModifyEndInternal(Handle);
        }

        internal void KeySetValue(int pKeyIndex, float value)
        {
            KeySetValueInternal(Handle, pKeyIndex, value);
        }

        internal void KeySetInterpolation(int pKeyIndex, EInterpolationType interpolationType)
        {
            KeySetInterpolationInternal(Handle, pKeyIndex, interpolationType);
        }

        public FbxAnimCurve(nint InHandle)
            : base(InHandle)
        {
        }
        public FbxAnimCurve(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }
    }
}
