using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxTexture : FbxObject
    {
        /** \enum EUnifiedMappingType      Internal enum for texture mapping types.
	    * Includes mapping types and planar mapping normal orientations.
		* Use SetMappingType(), GetMappingType(), SetPlanarMappingNormal() 
		* and GetPlanarMappingNormal() to access these values.
	    */
        public enum EUnifiedMappingType
        {
            eUMT_UV,            //! Maps to EMappingType::eUV.
            eUMT_XY,            //! Maps to EMappingType::ePlanar and EPlanarMappingNormal::ePlanarNormalZ.
            eUMT_YZ,            //! Maps to EMappingType::ePlanar and EPlanarMappingNormal::ePlanarNormalX.
            eUMT_XZ,            //! Maps to EMappingType::ePlanar and EPlanarMappingNormal::ePlanarNormalY.
            eUMT_SPHERICAL,     //! Maps to EMappingType::eSpherical.
            eUMT_CYLINDRICAL,   //! Maps to EMappingType::eCylindrical.
            eUMT_ENVIRONMENT,   //! Maps to EMappingType::eEnvironment.
            eUMT_PROJECTION,    //! Unused.
            eUMT_BOX,           //! DEPRECATED! Maps to EMappingType::eBox.
            eUMT_FACE,          //! DEPRECATED! Maps to EMappingType::eFace.
            eUMT_NO_MAPPING,    //! Maps to EMappingType::eNull.
        };

        /** \enum ETextureUse6         Internal enum for texture usage.
          * For example, the texture might be used as a standard texture, as a shadow map, as a bump map, etc.
          * Use SetTextureUse() and GetTextureUse() to access these values.	
          */
        public enum ETextureUse6
        {
            eTEXTURE_USE_6_STANDARD,                //! Maps to ETextureUse::eStandard.
            eTEXTURE_USE_6_SPHERICAL_REFLEXION_MAP, //! Maps to ETextureUse::eSphericalReflectionMap.
            eTEXTURE_USE_6_SPHERE_REFLEXION_MAP,    //! Maps to ETextureUse::eSphereReflectionMap.
            eTEXTURE_USE_6_SHADOW_MAP,              //! Maps to ETextureUse::eShadowMap.
            eTEXTURE_USE_6_LIGHT_MAP,               //! Maps to ETextureUse::eLightMap.
            eTEXTURE_USE_6_BUMP_NORMAL_MAP          //! Maps to ETextureUse::eBumpNormalMap.
        };

        /** \enum EWrapMode Wrap modes.
		  * Use SetWrapMode(), GetWrapModeU() and GetWrapModeV() to access these values.
		  */
        public enum EWrapMode
        {
            eRepeat,    //! Apply the texture over and over on the model's surface until the model is covered. This is the default setting.
            eClamp      //! Apply the texture to a model only once, using the color at the ends of the texture as the "filter".
        };

        /** \enum EBlendMode Blend modes.
		  */
        public enum EBlendMode
        {
            eTranslucent,   //! The texture is transparent, depending on the Alpha settings.
            eAdditive,      //! The color of the texture is added to the previous texture.
            eModulate,      //! The color value of the texture is multiplied by the color values of all previous layers of texture.
            eModulate2,     //! The color value of the texture is multiplied by two and then multiplied by the color values of all previous layers of texture.
            eOver			//! The texture is opaque.
        };



        /** \enum EAlignMode Align indices for cropping.
          */
        public enum EAlignMode
        {
            eLeft,	//! Left cropping.
            eRight,	//! Right cropping.
            eTop,	//! Top cropping.
            eBottom	//! Bottom cropping.
        };

        /** \enum ECoordinates Texture coordinates.
          */
        public enum ECoordinates
        {
            eU,	//! U axis.
            eV,	//! V axis.
            eW	//! W axis.
        };

        /** \enum ETextureUse           Texture uses.
  */
        public enum ETextureUse
        {
            eStandard,                  //! Standard texture use (ex. image)
            eShadowMap,                 //! Shadow map
            eLightMap,                  //! Light map
            eSphericalReflectionMap,    //! Spherical reflection map: Object reflects the contents of the scene
            eSphereReflectionMap,       //! Sphere reflection map: Object reflects the contents of the scene from only one point of view
            eBumpNormalMap              //! Bump map: Texture contains two direction vectors, that are used to convey relief in a texture.
        };

        /** \enum EMappingType Texture mapping types.
  */
        public enum EMappingType
        {
            eNull,          //! No texture mapping defined.
            ePlanar,        //! Apply texture to the model viewed as a plane.
            eSpherical,     //! Wrap texture around the model as if it was a sphere.
            eCylindrical,   //! Wrap texture around the model as if it was a cylinder.
            eBox,           //! Wrap texture around the model as if it was a box.
            eFace,          //! Apply texture to the model viewed as a face.
            eUV,            //! Apply texture to the model according to UVs.
            eEnvironment    //! Texture is an environment map.
        };

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetMappingType@FbxTexture@fbxsdk@@QEAAXW4EMappingType@12@@Z")]
        private static extern void SetMappingTypeInternal(nint pHandle, EMappingType pMappingType);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetSwapUV@FbxTexture@fbxsdk@@QEAAX_N@Z")]
        private static extern void SetSwapUVInternal(nint pHandle, bool pSwapUV);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetTranslation@FbxTexture@fbxsdk@@QEAAXNN@Z")]
        private static extern void SetTranslationInternal(nint pHandle, double pU, double pV);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetRotation@FbxTexture@fbxsdk@@QEAAXNNN@Z")]
        private static extern void SetRotationInternal(nint pHandle, double pU, double pV, double pW);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetScale@FbxTexture@fbxsdk@@QEAAXNN@Z")]
        private static extern void SetScaleInternal(nint pHandle, double pU, double pV);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetTextureUse@FbxTexture@fbxsdk@@QEAAXW4ETextureUse@12@@Z")]
        private static extern void SetTextureUseInternal(nint pHandle, ETextureUse pTextureUse);
        public FbxTexture(nint handle)
            : base(handle)
        {
        }

        internal void SetSwapUV(bool pSwapUV)
        {
            SetSwapUVInternal(pHandle, pSwapUV);
        }


        internal void SetTranslation(double pU, double pV)
        {
            SetTranslationInternal(pHandle, pU, pV);
        }


        internal void SetRotation(double pU, double pV, double pW = 0.0)
        {
            SetRotationInternal(pHandle, pU, pV, pW);
        }


        internal void SetScale(double pU, double pV)
        {
            SetScaleInternal(pHandle, pU, pV);
        }


        internal void SetTextureUse(ETextureUse pTextureUse)
        {
            SetTextureUseInternal(pHandle, pTextureUse);
        }


        internal void SetMappingType(EMappingType pMappingType)
        {
            SetMappingTypeInternal(pHandle, pMappingType);
        }
    }
}
