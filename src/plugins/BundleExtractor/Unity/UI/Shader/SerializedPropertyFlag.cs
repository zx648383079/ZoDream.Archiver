using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    [Flags]
    internal enum SerializedPropertyFlag
    {
        HideInInspector = 1 << 0,
        PerRendererData = 1 << 1,
        NoScaleOffset = 1 << 2,
        Normal = 1 << 3,
        HDR = 1 << 4,
        Gamma = 1 << 5,
        NonModifiableTextureData = 1 << 6,
        MainTexture = 1 << 7,
        MainColor = 1 << 8,
    }

}
