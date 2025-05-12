using ZoDream.BundleExtractor.Unity.Exporters;

namespace ZoDream.BundleExtractor.Unity
{
    public interface IScriptScheme
    {
        /// <summary>
        /// 基于脚本类名判断导出方式
        /// </summary>
        /// <param name="entryId"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        internal IMultipartExporter? Match(int entryId, ISerializedFile resource);
    }
}
