using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundleExtractor.Models
{
    [Flags]
    public enum UnityVersionFormatFlags
    {
        //
        // 摘要:
        //     The default flags when formatting.
        Default = 0,
        //
        // 摘要:
        //     Exclude AssetRipper.Primitives.UnityVersion.Type, AssetRipper.Primitives.UnityVersion.TypeNumber,
        //     and the custom engine number when formatting.
        ExcludeType = 1,
        //
        // 摘要:
        //     Format chinese versions as 2019.4.3c1 rather than 2019.4.3f1c1
        //
        // 言论：
        //     An example of the long version can be found in 'unity editor resources' and 'unity_builtin_extra'
        //     for the 2019.4.40f1c1 version.
        //     An example of the short version can be found in https://github.com/AssetRipper/AssetRipper/issues/841.
        UseShortChineseFormat = 2
    }
}
