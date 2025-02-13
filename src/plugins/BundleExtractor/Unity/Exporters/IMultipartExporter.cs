using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal interface IMultipartExporter : IFileExporter
    {

        public bool IsEmpty { get; }

        public string FileName { get; }
        public void Append(GameObject obj);
        public void Append(Mesh mesh);
        public void Append(Animator animator);
        public void Append(AnimationClip animator);
    }
}
