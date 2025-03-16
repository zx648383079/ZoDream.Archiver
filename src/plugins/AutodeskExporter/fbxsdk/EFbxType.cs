namespace ZoDream.AutodeskExporter
{
    internal enum EFbxType
    {
        eFbxUndefined,          //!< Unidentified.
        eFbxChar,               //!< 8 bit signed integer.
        eFbxUChar,              //!< 8 bit unsigned integer.
        eFbxShort,              //!< 16 bit signed integer.
        eFbxUShort,             //!< 16 bit unsigned integer.
        eFbxUInt,               //!< 32 bit unsigned integer.
        eFbxLongLong,           //!< 64 bit signed integer.
        eFbxULongLong,          //!< 64 bit unsigned integer.
        eFbxHalfFloat,          //!< 16 bit floating point.
        eFbxBool,               //!< Boolean.
        eFbxInt,                //!< 32 bit signed integer.
        eFbxFloat,              //!< Floating point value.
        eFbxDouble,             //!< Double width floating point value.
        eFbxDouble2,            //!< Vector of two double values.
        eFbxDouble3,            //!< Vector of three double values.
        eFbxDouble4,            //!< Vector of four double values.
        eFbxDouble4x4,          //!< Four vectors of four double values.
        eFbxEnum = 17,  //!< Enumeration.
        eFbxEnumM = -17,    //!< Enumeration allowing duplicated items.
        eFbxString = 18,    //!< String.
        eFbxTime,               //!< Time value.
        eFbxReference,          //!< Reference to object or property.
        eFbxBlob,               //!< Binary data block type.
        eFbxDistance,           //!< Distance.
        eFbxDateTime,           //!< Date and time.
        eFbxTypeCount = 24	//!< Indicates the number of type identifiers constants.
    };
}
