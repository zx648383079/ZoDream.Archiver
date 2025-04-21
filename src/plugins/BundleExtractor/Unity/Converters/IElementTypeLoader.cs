using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    /// <summary>
    /// 可以根据类型字典库读取
    /// </summary>
    internal interface IElementTypeLoader
    {

        public void Read(IBundleBinaryReader reader, TypeTree typeMaps);
    }
}
