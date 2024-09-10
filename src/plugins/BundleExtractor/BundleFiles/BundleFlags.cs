using System;

namespace ZoDream.BundleExtractor.BundleFiles
{
    [Flags]
    public enum BundleFlags
    {
        None = 0,

        CompressionBit1 = 0x1,
        CompressionBit2 = 0x2,
        CompressionBit3 = 0x4,
        CompressionBit4 = 0x8,
        CompressionBit5 = 0x10,
        CompressionBit6 = 0x20,
        CompressionTypeMask = 0x3F,

        BlocksAndDirectoryInfoCombined = 0x40,
        BlocksInfoAtTheEnd = 0x80,
        OldWebPluginCompatibility = 0x100,
        /// <summary>
        /// Padding is added after blocks info, so files within asset bundles start on aligned boundaries.
        /// </summary>
        /// <remarks>
        /// Introduced in 2020.3.34f1, 2021.3.2f1, 2022.1.1f1 so that Switch patching works appropriately.<br/>
        /// <see href="https://unity3d.com/unity/whats-new/2021.3.2"/><br/>
        /// <see href="https://issuetracker.unity3d.com/issues/files-within-assetbundles-do-not-start-on-aligned-boundaries-breaking-patching-on-nintendo-switch"/><br/>
        /// This fix implies that loading newly generated AssetBundles will require using this new Unity editor/runtime combination. It is not backwards compatible.
        /// </remarks>
        BlockInfoNeedPaddingAtStart = 0x200,
        /// <summary>
        /// Chinese encryption flag prior to 2020.3.34f1, 2021.3.2f1, 2022.1.1f1.
        /// </summary>
        EncryptionOld = 0x200,
        /// <summary>
        /// Chinese encryption flag (presumeably) after 2020.3.34f1, 2021.3.2f1, 2022.1.1f1.
        /// </summary>
        EncryptionNew = 0x400,
    }
}
