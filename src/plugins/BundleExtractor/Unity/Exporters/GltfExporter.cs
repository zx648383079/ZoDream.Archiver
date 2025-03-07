using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.KhronosExporter;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Collections;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using ZoDream.Shared.Numerics;
using Mesh = ZoDream.KhronosExporter.Models.Mesh;
using UnityMesh = ZoDream.BundleExtractor.Unity.UI.Mesh;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class GltfExporter : IMultipartExporter
    {
        public GltfExporter()
        {
            _root = new();
            _root.Scenes.Add(new());
        }

        private readonly ModelSource _root;

        public bool IsEmpty => _root.Nodes.Count == 0;

        public string FileName => string.Empty;

        public void Append(GameObject obj)
        {
            var sceneIndex = _root.Scenes.AddWithIndex(new()
            {
                Name = obj.m_Name
            });
            AddNode(obj.m_Transform, sceneIndex, -1);
        }

        private void AddNode(Transform trans, int sceneIndex, int parentIndex)
        {
            var nodeIndex = parentIndex;
            if (!trans.m_GameObject.TryGet(out var game))
            {
                var res = AddNode(game.m_MeshRenderer, sceneIndex, parentIndex);
                // var skin = AddNode(obj.m_SkinnedMeshRenderer, sceneIndex);
                if (res >= 0)
                {
                    _root.Nodes[res].Scale = trans.m_LocalScale.AsArray();
                    _root.Nodes[res].Translation = trans.m_LocalPosition.AsArray();
                    _root.Nodes[res].Rotation = trans.m_LocalRotation.AsArray();
                    nodeIndex = res;
                }
            }
            foreach (var pptr in trans.m_Children)
            {
                if (pptr.TryGet(out var t))
                {
                    AddNode(t, sceneIndex, nodeIndex);
                }
            }
        }

        private int AddNode(UIRenderer renderer, int sceneIndex, int parentIndex)
        {
            if (renderer is SkinnedMeshRenderer sMesh)
            {
                if (sMesh.m_Mesh.TryGet(out var m_Mesh))
                {
                    return AddNode(m_Mesh, sceneIndex, parentIndex);
                }
            }
            else
            {
                if (renderer.m_GameObject.TryGet(out var m_GameObject) &&
                    m_GameObject.m_MeshFilter != null)
                {
                    if (m_GameObject.m_MeshFilter.m_Mesh.TryGet(out var m_Mesh))
                    {
                        return AddNode(m_Mesh, sceneIndex, parentIndex);
                    }
                }
            }
            return -1;
        }

        private int AddNode(UnityMesh mesh, int sceneIndex, int parentIndex)
        {
            var ps = new MeshPrimitive()
            {
                Mode = PrimitiveType.TRIANGLES
            };
            ps.Indices = _root.CreateIndicesAccessor(mesh.m_Name + "_indices");
            _root.AddAccessorBuffer(ps.Indices, mesh.m_Indices.ToArray());
            if (mesh.m_VertexCount > 0)
            {
                ps.Attributes.Add("POSITION",
                    _root.CreateVectorAccessor(mesh.m_Name + "_positions",
                    mesh.m_Vertices, mesh.m_VertexCount));
            }
            if (mesh.m_Normals?.Length > 0)
            {
                ps.Attributes.Add("NORMAL",
                    _root.CreateVectorAccessor(mesh.m_Name + "_normals",
                    mesh.m_Normals, mesh.m_VertexCount));
            }
            for (int i = 0; i <= 7; i++)
            {
                var uvs = mesh.GetUV(i);
                if (uvs?.Length > 0)
                {
                    ps.Attributes.Add("TEXCOORD_" + i,
                        _root.CreateVectorAccessor(mesh.m_Name + "_texcoords",
                        uvs, mesh.m_VertexCount));
                }
            }
            var meshIndex = _root.Meshes.AddWithIndex(new Mesh()
            {
                Name = mesh.m_Name,
                Primitives = {
                    ps
                }
            });
            var nodeIndex = _root.Nodes.AddWithIndex(new()
            {
                Name = mesh.m_Name,
                Mesh = meshIndex
            });
            if (parentIndex >= 0)
            {
                _root.Nodes[parentIndex].Children.Add(nodeIndex);
            }
            _root.Scenes[sceneIndex].Nodes.Add(nodeIndex);
            return nodeIndex;
        }

        private int AddNode(Skeleton mesh, int sceneIndex, int parentIndex)
        {
            var nodeTree = new Dictionary<int, List<int>>();
            for (int i = 0; i < mesh.m_Node.Count; i++)
            {
                var item = mesh.m_Node[i];
                var pIndex = Array.IndexOf(mesh.m_ID, item.m_ParentId);
                if (nodeTree.TryGetValue(pIndex, out var box))
                {
                    box.Add(i);
                    continue;
                }
                nodeTree.Add(pIndex, [i]);
            }
            return AddNode(mesh, nodeTree, -1, sceneIndex, parentIndex).FirstOrDefault();
        }

        private int[] AddNode(Skeleton mesh, Dictionary<int, List<int>> nodeTree, int pIndex, int sceneIndex, int parentIndex)
        {
            if (!nodeTree.TryGetValue(pIndex, out var items))
            {
                return [-1];
            }
            var res = new int[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                res[i] = AddNode(mesh, items[i], sceneIndex, parentIndex);
                AddNode(mesh, nodeTree, items[i], sceneIndex, res[i]);
            }
            return res;
        }

        private int AddNode(Skeleton mesh, int index, int sceneIndex, int parentIndex)
        {
            var ps = new MeshPrimitive()
            {
                Mode = PrimitiveType.POINTS
            };
            ps.Indices = _root.CreateIndicesAccessor("skeleton_indices");
            _root.AddAccessorBuffer(ps.Indices, 0);
            var axes = mesh.m_AxesArray[mesh.m_Node[index].m_AxesId];
            if (axes is not null)
            {
                ps.Attributes.Add("POSITION", _root.CreateVectorAccessor("skeleton_vectors", axes.m_PreQ.AsArray(), 4));
                ps.Attributes.Add("JOINTS_0", _root.CreateVectorAccessor("skeleton_vectors", axes.m_PostQ.AsArray(), 4));
                ps.Attributes.Add("WEIGHTS_0", _root.CreateVectorAccessor("skeleton_vectors",
                    axes.m_Sgn is Vector4 o ? o.AsArray() : [..((Vector3)axes.m_Sgn).AsArray(), axes.m_Length], 
                4));
            }
            
            var meshIndex = _root.Meshes.AddWithIndex(new Mesh()
            {
                Name = $"skeleton_{mesh.m_ID[index]}",
                Primitives = {
                    ps
                }
            });
            var nodeIndex = _root.Nodes.AddWithIndex(new()
            {
                Name = $"skeleton_{mesh.m_ID[index]}",
                Mesh = meshIndex
            });
            if (parentIndex >= 0)
            {
                _root.Nodes[parentIndex].Children.Add(nodeIndex);
            }
            _root.Scenes[sceneIndex].Nodes.Add(nodeIndex);
            return nodeIndex;
        }

        public void Append(UnityMesh mesh)
        {
            AddNode(mesh, _root.Scene, -1);
        }

        public void Append(Animator animator)
        {
            
        }

        public void Append(AnimationClip animator)
        {

        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (IsEmpty || !LocationStorage.TryCreate(fileName, ".glb", mode, out fileName))
            {
                return;
            }
            using var fs = File.OpenWrite(fileName);
            new GlbWriter().Write(_root, fs);
        }

        public void Dispose()
        {
            _root.Dispose();
        }
    }
}
