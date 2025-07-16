namespace UnityEngine
{
    public class AssetBundleManifest : Object
    {
        public string[] AssetBundleNames { get; set; } = [];

        public AssetBundleInfo[] AssetBundleInfos { get; set; } = [];
    }

    public struct AssetBundleInfo
    {
        public byte[] AssetBundleHash;

        public int[] AssetBundleDependencies;
    }
}
