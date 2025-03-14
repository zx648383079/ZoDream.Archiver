namespace ZoDream.AutodeskExporter
{
    internal enum EStatusCode
    {
        eSuccess = 0,                           //!< Operation was successful
        eFailure,                               //!< Operation failed
        eInsufficientMemory,                    //!< Operation failed due to insufficient memory
        eInvalidParameter,                      //!< An invalid parameter was provided
        eIndexOutOfRange,                       //!< Index value outside the valid range
        ePasswordError,                         //!< Operation on FBX file password failed
        eInvalidFileVersion,                    //!< File version not supported (anymore or yet)
        eInvalidFile                            //!< Operation on the file access failed
    };
}
