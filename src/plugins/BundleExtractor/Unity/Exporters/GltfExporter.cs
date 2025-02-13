using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class GltfExporter : IMultipartExporter
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
    }
}
