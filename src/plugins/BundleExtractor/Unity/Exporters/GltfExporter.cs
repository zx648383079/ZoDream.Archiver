using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Collections;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class GltfExporter : IMultipartExporter
    {
        public GltfExporter()
        {
            _root = new();
            _root.Scenes.Add(new());
        }

        private readonly KhronosExporter.Models.ModelRoot _root;

        public bool IsEmpty => true;

        public string FileName => string.Empty;

        public void Append(GameObject obj)
        {
        }

        public void Append(Mesh mesh)
        {
            var meshIndex = _root.Meshes.AddWithIndex(new KhronosExporter.Models.Mesh()
            {
                Name = mesh.m_Name,
                Primitives = 
            });
            var nodeIndex = _root.Nodes.AddWithIndex(new (){ 
                Name = mesh.m_Name, 
                Mesh = meshIndex
            });
            _root.Scenes[_root.Scene].Nodes.Add(nodeIndex);
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
