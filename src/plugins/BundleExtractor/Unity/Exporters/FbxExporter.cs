using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class FbxExporter : IMultipartExporter
    {
        public bool IsEmpty => true;
        public string FileName => string.Empty;
        public void Append(GameObject obj)
        {
        }

        public void Append(Mesh mesh)
        {
        }

        public void Append(Animator animator)
        {
        }

        public void Append(AnimationClip animator)
        {
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            
        }
        public void Dispose()
        {
        }

    }
}
