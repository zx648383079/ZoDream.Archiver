using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.Exporters;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class GameObject(UIReader reader) : EditorExtension(reader), IFileExporter
    {
        public List<PPtr<UIComponent>> m_Components;
        public string m_Name;

        public Transform m_Transform;
        public MeshRenderer m_MeshRenderer;
        public MeshFilter m_MeshFilter;
        public SkinnedMeshRenderer m_SkinnedMeshRenderer;
        public Animator m_Animator;
        public Animation m_Animation;

        public override string Name => m_Name;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var version = reader.Get<UnityVersion>();
            int m_Component_size = reader.ReadInt32();
            m_Components = [];
            for (int i = 0; i < m_Component_size; i++)
            {
                if (version.LessThan(5, 5)) //5.5 down
                {
                    int first = reader.ReadInt32();
                }
                m_Components.Add(new PPtr<UIComponent>(reader));
            }

            var m_Layer = reader.ReadInt32();
            m_Name = reader.ReadAlignedString();
        }

        public bool HasModel() => HasMesh(m_Transform, new List<bool>());
        private static bool HasMesh(Transform m_Transform, List<bool> meshes)
        {
            try
            {
                m_Transform.m_GameObject.TryGet(out var m_GameObject);

                if (m_GameObject.m_MeshRenderer != null)
                {
                    var mesh = GetMesh(m_GameObject.m_MeshRenderer);
                    meshes.Add(mesh != null);
                }

                if (m_GameObject.m_SkinnedMeshRenderer != null)
                {
                    var mesh = GetMesh(m_GameObject.m_SkinnedMeshRenderer);
                    meshes.Add(mesh != null);
                }

                foreach (var pptr in m_Transform.m_Children)
                {
                    if (pptr.TryGet(out var child))
                        meshes.Add(HasMesh(child, meshes));
                }

                return meshes.Any(x => x == true);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to verify if {m_Transform?.Name} has meshes, skipping...");
                return false;
            }
        }

        private static Mesh GetMesh(UIRenderer meshR)
        {
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                if (sMesh.m_Mesh.TryGet(out var m_Mesh))
                {
                    return m_Mesh;
                }
            }
            else
            {
                if (meshR.m_GameObject.TryGet(out var m_GameObject) && 
                    m_GameObject.m_MeshFilter != null)
                {
                    if (m_GameObject.m_MeshFilter.m_Mesh.TryGet(out var m_Mesh))
                    {
                        return m_Mesh;
                    }
                }
            }

            return null;
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            // TODO fbx
            new FbxExporter(this, SkiaSharp.SKEncodedImageFormat.Bmp)
                .SaveAs(fileName, mode);
        }
    }
}
