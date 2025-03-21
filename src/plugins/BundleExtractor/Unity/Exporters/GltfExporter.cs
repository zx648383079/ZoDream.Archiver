using SharpCompress.Compressors.Xz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using ZoDream.AutodeskExporter;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.KhronosExporter;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Collections;
using ZoDream.Shared.Models;
using ZoDream.Shared.Numerics;
using ZoDream.Shared.Storage;
using Animation = ZoDream.KhronosExporter.Models.Animation;
using Material = ZoDream.KhronosExporter.Models.Material;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Mesh = ZoDream.KhronosExporter.Models.Mesh;
using Node = ZoDream.KhronosExporter.Models.Node;
using UnityMaterial = ZoDream.BundleExtractor.Unity.UI.Material;
using UnityMesh = ZoDream.BundleExtractor.Unity.UI.Mesh;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    /// <summary>
    /// 存在问题， 不确定 node 与 frame 通过 name 联系？
    /// </summary>
    internal class GltfExporter : IMultipartExporter
    {
        public GltfExporter(IBundleContainer container)
        {
            _container = container;
            _root = new();
            _root.Add(new Scene());
        }

        private readonly IBundleContainer _container;
        private bool IsBinaryFile => _container.Options?.ModelFormat == "glb";
        private readonly ModelSource _root;
        private readonly Dictionary<string, IFileExporter> _attachItems = [];
        private readonly Dictionary<string, int> _nodeItems = [];
        private readonly Dictionary<string, int> _materialItems = [];
        private readonly HashSet<AnimationClip> _animationItems = [];
        private readonly Dictionary<uint, string> _morphChannelNames = [];
        private readonly Dictionary<uint, string> _bonePathHash = [];
        private readonly Dictionary<string, string> _channelPathItems = [];
        private readonly Dictionary<Transform, string> _transformItems = [];

        private Avatar? _avatar;
        private long _entryFileId;

        public bool IsEmpty => _root.Nodes.Count == 0;
        public string Name { get; private set; } = string.Empty;

        public void Append(GameObject obj)
        {
            _entryFileId = obj.FileID;
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(obj.Name))
            {
                Name = obj.Name;
            }
            if (obj.m_Animator is not null)
            {
                AddAnimator(obj.m_Animator);
            }
            else
            {
                AddGame(obj);
            }
            //var sceneIndex = _root.Scenes.AddWithIndex(new()
            //{
            //    Name = obj.m_Name
            //});
            //AddNode(obj.m_Transform, sceneIndex, -1);
        }

        public void Append(UnityMesh mesh)
        {
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(mesh.Name))
            {
                Name = mesh.Name;
            }
            if (_nodeItems.ContainsKey(mesh.m_Name))
            {
                return;
            }
            //if (string.IsNullOrEmpty(FileName))
            //{
            //    FileName = mesh.m_Name;
            //}
            //AddNode(mesh, _root.Scene, -1);
        }

        public void Append(Animator animator)
        {
            _entryFileId = animator.FileID;
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(animator.Name))
            {
                Name = animator.Name;
            }
            AddAnimator(animator);
        }

        public void Append(AnimationClip animator)
        {
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(animator.Name))
            {
                Name = animator.Name;
            }
            _animationItems.Add(animator);
        }


        private void AddAnimator(Animator animator)
        {
            if (animator.m_Avatar.TryGet(out var m_Avatar))
            {
                _avatar = m_Avatar;
            }
            _container.TryAddExclude(animator.m_GameObject.m_PathID);
            if (animator.m_GameObject.TryGet(out var game))
            {
                AddGame(game);
            }
        }
        private void AddGame(GameObject game, bool hasTransformHierarchy = true)
        {
            var m_Transform = game.m_Transform;
            //if (!hasTransformHierarchy)
            //{
            //    AddTransforms(m_Transform);
            //}
            //else
            //{
            //    var tempTransform = m_Transform;
            //    while (tempTransform.m_Father.TryGet(out var m_Father))
            //    {
            //        AddTransform(m_Father);
            //        tempTransform = m_Father;
            //    }
            //    AddTransforms(m_Transform);

            //    CreateBonePathHash(m_Transform);
            //}

            AddMeshRenderer(m_Transform);
            CreateBonePathHash(m_Transform);
        }

        private void AddMeshRenderer(Transform m_Transform, int meshParent = -1)
        {
            m_Transform.m_GameObject.TryGet(out var m_GameObject);
            _container.TryAddExclude(m_Transform.m_GameObject.m_PathID);
            if (m_GameObject?.m_MeshRenderer != null)
            {
                meshParent = AddMeshRenderer(m_GameObject.m_MeshRenderer, meshParent);
            }

            if (m_GameObject?.m_SkinnedMeshRenderer != null)
            {
                AddMeshRenderer(m_GameObject.m_SkinnedMeshRenderer);
            }

            if (m_GameObject?.m_Animation != null)
            {
                foreach (var animation in m_GameObject.m_Animation.m_Animations)
                {
                    if (animation.TryGet(out var animationClip))
                    {
                        //if (!boundAnimationPathDic.ContainsKey(animationClip))
                        //{
                        //    boundAnimationPathDic.Add(animationClip, GetTransformPath(m_Transform));
                        //}
                        _animationItems.Add(animationClip);
                    }
                }
            }

            foreach (var pptr in m_Transform.m_Children)
            {
                _container.TryAddExclude(pptr.m_PathID);
                if (pptr.TryGet(out var child))
                {
                    AddMeshRenderer(child, meshParent);
                }
            }
        }

        private int AddTransform(Transform trans)
        {
            trans.m_GameObject.TryGet(out var m_GameObject);
            var nodeIndex = FindNode(m_GameObject?.m_Name, -1);
            _transformItems.Add(trans, m_GameObject?.m_Name);
            UpdateNode(nodeIndex, trans.m_LocalPosition, trans.m_LocalRotation, trans.m_LocalScale);
            return nodeIndex;
        }

        private void UpdateNode(int nodeIndex, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (nodeIndex < 0)
            {
                return;
            }
            UpdateNode(_root.Nodes[nodeIndex], position, rotation, scale);
        }
        private void UpdateNode(Node node, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            node.Translation = [ -position.X, position.Y, position.Z];
            node.Rotation = [rotation.X, -rotation.Y, -rotation.Z, rotation.W];
            node.Scale = scale.AsArray();
        }

        private void AddTransforms(Transform trans, int nodeParent = -1)
        {
            var nodeIndex = AddTransform(trans);
            if (nodeParent < 0)
            {
                // RootFrame = frame;
            }
            else
            {
                // parent.AddChild(frame);
            }
            foreach (var pptr in trans.m_Children)
            {
                if (pptr.TryGet(out var child))
                {
                    AddTransforms(child, nodeIndex);
                }
            }
        }

        private int AddMeshRenderer(UIRenderer meshR, int meshParent = -1)
        {
            var mesh = GetMesh(meshR);
            if (mesh == null)
            {
                return -1;
            }
            var nodeIndex = AddNode(mesh, 0, meshParent, meshR);
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                //Bone
                /*
                 * 0 - None
                 * 1 - m_Bones
                 * 2 - m_BoneNameHashes
                 */
                var boneType = 0;
                if (sMesh.m_Bones.Count > 0)
                {
                    if (sMesh.m_Bones.Count == mesh.m_BindPose.Length)
                    {
                        var verifiedBoneCount = sMesh.m_Bones.Count(x => x.TryGet(out _));
                        if (verifiedBoneCount > 0)
                        {
                            boneType = 1;
                        }
                        if (verifiedBoneCount != sMesh.m_Bones.Count)
                        {
                            //尝试使用m_BoneNameHashes 4.3 and up
                            if (mesh.m_BindPose.Length > 0 && (mesh.m_BindPose.Length == mesh.m_BoneNameHashes?.Length))
                            {
                                //有效bone数量是否大于SkinnedMeshRenderer
                                var verifiedBoneCount2 = mesh.m_BoneNameHashes.Count(x => GetPathFromHash(x) != null);
                                if (verifiedBoneCount2 > verifiedBoneCount)
                                {
                                    boneType = 2;
                                }
                            }
                        }
                    }
                }
                if (boneType == 0)
                {
                    //尝试使用m_BoneNameHashes 4.3 and up
                    if (mesh.m_BindPose.Length > 0 && (mesh.m_BindPose.Length == mesh.m_BoneNameHashes?.Length))
                    {
                        var verifiedBoneCount = mesh.m_BoneNameHashes.Count(x => GetPathFromHash(x) != null);
                        if (verifiedBoneCount > 0)
                        {
                            boneType = 2;
                        }
                    }
                }

                if (boneType == 1)
                {
                    var boneCount = sMesh.m_Bones.Count;
                    var joinItems = new int[boneCount];
                    var matrixItems = new Matrix4x4[boneCount];
                    for (int i = 0; i < boneCount; i++)
                    {
                        if (sMesh.m_Bones[i].TryGet(out var m_Transform))
                        {
                            joinItems[i] = FindNode(GetTransformPath(m_Transform), nodeIndex);
                        }
                        var convert = Matrix4x4.CreateScale(new Vector3(-1, 1, 1));
                        matrixItems[i] = convert * mesh.m_BindPose[i] * convert;
                    }
                    _root.Nodes[nodeIndex].Skin = _root.Add(new Skin()
                    {
                        InverseBindMatrices = _root.CreateAccessor($"skins_{nodeIndex}", matrixItems),
                        Joints = joinItems,
                        Skeleton = nodeIndex,
                    });
                }
                else if (boneType == 2)
                {
                    var boneCount = mesh.m_BindPose.Length;
                    var joinItems = new int[boneCount];
                    var matrixItems = new Matrix4x4[boneCount];
                    for (int i = 0; i < boneCount; i++)
                    {
                        var bone = new FbxImportedBone();
                        var boneHash = mesh.m_BoneNameHashes[i];
                        var path = GetPathFromHash(boneHash);
                        joinItems[i] = FindNode(path, nodeIndex);
                        var convert = Matrix4x4.CreateScale(new Vector3(-1, 1, 1));
                        matrixItems[i] = convert * mesh.m_BindPose[i] * convert;
                    }
                    _root.Nodes[nodeIndex].Skin = _root.Add(new Skin()
                    {
                        InverseBindMatrices = _root.CreateAccessor($"skins_{nodeIndex}", matrixItems),
                        Joints = joinItems,
                        Skeleton = nodeIndex,
                    });
                }

                //Morphs
                if (mesh.m_Shapes?.channels?.Count > 0)
                {
                    meshR.m_GameObject.TryGet(out var m_GameObject2);
                    _channelPathItems[mesh.Name] = GetTransformPath(m_GameObject2.m_Transform);
                    for (int i = 0; i < mesh.m_Shapes.channels.Count; i++)
                    {
                        var shapeChannel = mesh.m_Shapes.channels[i];
                        var blendShapeName = "blendShape." + shapeChannel.name;
                        var bytes = Encoding.UTF8.GetBytes(blendShapeName);
                        _morphChannelNames[Crc32.Compute(bytes)] = blendShapeName;

                        var node = _root.Meshes[FindNode(shapeChannel.name, nodeIndex)];
                        node.Weights ??= [];
                        var frameEnd = shapeChannel.frameIndex + shapeChannel.frameCount;
                        for (int frameIdx = shapeChannel.frameIndex; frameIdx < frameEnd; frameIdx++)
                        {
                            node.Weights.Add(mesh.m_Shapes.fullWeights[frameIdx]);
                            var shape = mesh.m_Shapes.shapes[frameIdx];

                            var vertexEnd = shape.firstVertex + shape.vertexCount;
                            var vertItems = new float[shape.vertexCount * 3];
                            var normalItems = new float[shape.vertexCount * 3];
                            var tangentItems = new float[shape.vertexCount * 4];
                            for (uint j = shape.firstVertex; j < vertexEnd; j++)
                            {
                                var morphVertex = mesh.m_Shapes.vertices[(int)j];
                                
                                var morphPos = morphVertex.vertex;
                                vertItems.AddRange(-morphPos.X, morphPos.Y, morphPos.Z);
                                if (shape.hasNormals)
                                {
                                    var morphNormal = morphVertex.normal;
                                    normalItems.AddRange(-morphNormal.X, morphNormal.Y, morphNormal.Z);
                                }
                                if (shape.hasTangents)
                                {
                                    var morphTangent = morphVertex.tangent;
                                    tangentItems.AddRange(-morphTangent.X, morphTangent.Y, morphTangent.Z, 0);
                                }
                            }
                            var target = new Dictionary<string, int>()
                            {
                                {"POSITION", _root.CreateVectorAccessor("blendShapeName", vertItems, (int)shape.vertexCount) }
                            };
                            if (shape.hasNormals)
                            {
                                target.Add("NORMAL", _root.CreateVectorAccessor("blendShapeName", normalItems, (int)shape.vertexCount));
                            }
                            if (shape.hasTangents)
                            {
                                target.Add("TANGENT", _root.CreateVectorAccessor("blendShapeName", tangentItems, (int)shape.vertexCount));
                            }
                            // TODO 
                            (node.Primitives[0].Targets ??= []).Add(target);
                        }
                    }
                }
            }
            return nodeIndex;
        }

        private int FindNode(string name, int parent)
        {
            var items = _root.Nodes[parent].Children;
            if (items is null)
            {
                return parent;
            }
            foreach (var index in items)
            {
                if (_root.Nodes[index].Name == name)
                {
                    return index;
                }
            }
            // TODO ?
            return parent;
        }

        private static UnityMesh GetMesh(UIRenderer meshR)
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
                meshR.m_GameObject.TryGet(out var m_GameObject);
                if (m_GameObject.m_MeshFilter != null)
                {
                    if (m_GameObject.m_MeshFilter.m_Mesh.TryGet(out var m_Mesh))
                    {
                        return m_Mesh;
                    }
                }
            }

            return null;
        }

        private string GetTransformPath(Transform transform)
        {
            if (_transformItems.TryGetValue(transform, out var item))
            {
                return item;
            }
            return null;
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
                    UpdateNode(res, trans.m_LocalPosition, trans.m_LocalRotation, trans.m_LocalScale);
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
        /// <summary>
        /// 计算步进数
        /// </summary>
        /// <param name="total"></param>
        /// <param name="count"></param>
        /// <param name="rangeItems"></param>
        /// <returns></returns>
        private static int ComputeStep(int total, int count, [MinLength(1)] params int[] rangeItems)
        {
            foreach (var item in rangeItems)
            {
                if (count * item == total)
                {
                    return item;
                }
            }
            return rangeItems[0];
        }

        private int AddNode(UnityMesh mesh, int sceneIndex, int parentIndex, 
            UIRenderer? meshR = null)
        {
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(mesh.Name))
            {
                Name = mesh.Name + _entryFileId.ToString()[^3..];
            }
            if (_nodeItems.TryGetValue(mesh.m_Name, out var cacheIndex))
            {
                if (parentIndex >= 0)
                {
                    (_root.Nodes[parentIndex].Children ??= []).Add(cacheIndex);
                }
                return cacheIndex;
            }
            
            #region 转换 Mesh
            var psItems = new List<MeshPrimitive>();
            var hasUv = mesh.m_UV0 is not null && mesh.m_UV0.Length > 0;
            var vStep = ComputeStep(mesh.m_Vertices.Length, mesh.m_VertexCount, 3, 4);
            var uStep = ComputeStep(mesh.m_UV0?.Length??0, mesh.m_VertexCount, 4, 2, 3);
            var nStep = ComputeStep(mesh.m_Normals.Length, mesh.m_VertexCount, 3, 4);
            int firstSubMesh = 0;
            if (meshR is not null && meshR.m_StaticBatchInfo?.subMeshCount > 0)
            {
                firstSubMesh = meshR.m_StaticBatchInfo.firstSubMesh;
            }
            else if (meshR is not null && meshR.m_SubsetIndices?.Length > 0)
            {
                firstSubMesh = (int)meshR.m_SubsetIndices.Min(x => x);
            }
            int sum = 0;
            for (var i = 0; i < mesh.m_SubMeshes.Count; i++)
            {
                var indexCount = (int)mesh.m_SubMeshes[i].indexCount;
                var end = sum + indexCount / 3;
                int? materialIndex = null;
                if (meshR is not null && i - firstSubMesh < meshR.m_Materials.Count)
                {
                    if (meshR.m_Materials[i - firstSubMesh].TryGet(out var m_Material))
                    {
                        materialIndex = AddMaterial(m_Material);
                    }
                }

                var positionItems = new List<float>();
                var normalItems = new List<float>();
                var uvItems = CreateArray(8);
                var indicesItems = new List<int>();
                var faceVertexCache = new Dictionary<int, int>();
                var faceVertexCount = 0;

                for (int f = sum; f < end; f++)
                {
                    var v1 = (int)mesh.m_Indices[f * 3 + 2];
                    var v2 = (int)mesh.m_Indices[f * 3 + 1];
                    var v3 = (int)mesh.m_Indices[f * 3];

                    var p1 = new Vector3(-mesh.m_Vertices[v1 * vStep],
                            mesh.m_Vertices[v1 * vStep + 1],
                            mesh.m_Vertices[v1 * vStep + 2]);
                    var p2 = new Vector3(-mesh.m_Vertices[v2 * vStep],
                            mesh.m_Vertices[v2 * vStep + 1],
                            mesh.m_Vertices[v2 * vStep + 2]);
                    var p3 = new Vector3(-mesh.m_Vertices[v3 * vStep],
                            mesh.m_Vertices[v3 * vStep + 1],
                            mesh.m_Vertices[v3 * vStep + 2]);

                    var n1 = new Vector3(-mesh.m_Normals[v1 * nStep],
                            mesh.m_Normals[v1 * nStep + 1],
                            mesh.m_Normals[v1 * nStep + 2]);

                    if (!faceVertexCache.ContainsKey(v1))
                    {
                        faceVertexCache.Add(v1, faceVertexCount++);
                        positionItems.AddRange(p1.AsArray());
                        normalItems.AddRange(n1.AsArray());

                        if (hasUv)
                        {
                            for (int ui = 0; ui < uvItems.Length; ui++)
                            {
                                var uv = mesh.GetUV(ui);
                                if (uv is not null && uv.Length > 0)
                                {
                                    uvItems[ui].AddRange(
                                        uv[v1 * uStep],
                                        1 - uv[v1 * uStep + 1]
                                    );
                                }
                            }
                        }
                        
                    }

                    if (!faceVertexCache.ContainsKey(v2))
                    {
                        faceVertexCache.Add(v2, faceVertexCount++);
                        positionItems.AddRange(p2.AsArray());

                        normalItems.AddRange(
                            -mesh.m_Normals[v2 * nStep],
                            mesh.m_Normals[v2 * nStep + 1],
                            mesh.m_Normals[v2 * nStep + 2]
                         );

                        if (hasUv)
                        {
                            for (int ui = 0; ui < uvItems.Length; ui++)
                            {
                                var uv = mesh.GetUV(ui);
                                if (uv is not null && uv.Length > 0)
                                {
                                    uvItems[ui].AddRange(
                                        uv[v2 * uStep],
                                        1 - uv[v2 * uStep + 1]
                                    );
                                }
                            }
                        }
                    }
                    if (!faceVertexCache.ContainsKey(v3))
                    {
                        faceVertexCache.Add(v3, faceVertexCount++);
                        positionItems.AddRange(p3.AsArray());

                        normalItems.AddRange(
                            -mesh.m_Normals[v3 * nStep],
                            mesh.m_Normals[v3 * nStep + 1],
                            mesh.m_Normals[v3 * nStep + 2]
                         );

                        if (hasUv)
                        {
                            for (int ui = 0; ui < uvItems.Length; ui++)
                            {
                                var uv = mesh.GetUV(ui);
                                if (uv is not null && uv.Length > 0)
                                {
                                    uvItems[ui].AddRange(
                                        uv[v3 * uStep],
                                        1 - uv[v3 * uStep + 1]
                                    );
                                }
                            }
                        }
                    }

                    if (ObjReader.CheckWindingCorrect(p1, p2, p3, n1))
                    {
                        indicesItems.AddRange(
                            faceVertexCache[v1],
                            faceVertexCache[v2],
                            faceVertexCache[v3]
                        );
                    } else
                    {
                        indicesItems.AddRange(
                            faceVertexCache[v1],
                            faceVertexCache[v3],
                            faceVertexCache[v2]
                        );
                    }
                }

                var ps = new MeshPrimitive
                {
                    Mode = PrimitiveType.TRIANGLES,
                    Indices = _root.CreateIndicesAccessor($"{mesh.m_Name}_{i}_indices"),
                    Material = materialIndex
                };
                _root.AddAccessorBuffer(ps.Indices, indicesItems.ToArray());
                ps.Attributes.Add("POSITION",
                    _root.CreateVectorAccessor($"{mesh.m_Name}_{i}_positions",
                    positionItems.ToArray(), positionItems.Count / 3));
                if (normalItems.Count > 0)
                {
                    ps.Attributes.Add("NORMAL",
                    _root.CreateVectorAccessor($"{mesh.m_Name}_{i}_normals",
                        normalItems.ToArray(), normalItems.Count / 3));
                }
                for (int j = 0; j < uvItems.Length; j++)
                {
                    if (uvItems[j].Count > 0)
                    {
                        ps.Attributes.Add($"TEXCOORD_{j}",
                            _root.CreateVectorAccessor($"{mesh.m_Name}_{i}_{j}_texcoords",
                            uvItems[j].ToArray(), uvItems[j].Count / 2));
                    }
                }
                psItems.Add(ps);
                sum = end;
            }

            
            #endregion


            var meshIndex = _root.Meshes.AddWithIndex(new Mesh()
            {
                Name = mesh.m_Name,
                Primitives = psItems
            });
            var nodeIndex = _root.Nodes.AddWithIndex(new()
            {
                Name = mesh.m_Name,
                Mesh = meshIndex
            });
            _nodeItems.Add(mesh.m_Name, nodeIndex);
            if (parentIndex >= 0)
            {
                (_root.Nodes[parentIndex].Children ??= []).Add(nodeIndex);
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
            var ps = new MeshPrimitive
            {
                Mode = PrimitiveType.POINTS,
                Indices = _root.CreateIndicesAccessor("skeleton_indices")
            };
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
                (_root.Nodes[parentIndex].Children ??= []).Add(nodeIndex);
            }
            _root.Scenes[sceneIndex].Nodes.Add(nodeIndex);
            return nodeIndex;
        }

        private int AddMaterial(UnityMaterial mat)
        {
            if (_materialItems.TryGetValue(mat.Name, out var materialIndex))
            {
                return materialIndex;
            }
            var res = new Material()
            {
                Name = mat.m_Name,
                EmissiveFactor = new(0, 0, 0),
            };

            var pbr = new MaterialPBRSpecularGlossiness()
            {
                DiffuseFactor = new(0.8f, 0.8f, 0.8f, 1),
                SpecularFactor = new(0.2f, 0.2f, 0.2f),
                GlossinessFactor = 20
            };
            res.AddExtension(MaterialPBRSpecularGlossiness.ExtensionName, pbr);
            res.AddExtension(MaterialsIOR.ExtensionName, new MaterialsIOR()
            {
                Ior = 1000
            });
            _root.AddExtensionUsed(MaterialsIOR.ExtensionName);
            _root.AddExtensionUsed(MaterialPBRSpecularGlossiness.ExtensionName);
            foreach (var col in mat.m_SavedProperties.m_Colors)
            {
                switch (col.Key)
                {
                    case "_BaseColor":
                    case "_Color":
                         pbr.DiffuseFactor = col.Value;
                        break;
                    case "_SColor":
                        res.PbrMetallicRoughness.BaseColorFactor = col.Value;
                        //res.AlphaMode = AlphaMode.MASK;
                        break;
                    case "_EmissionColor":
                        res.EmissiveFactor = new(col.Value.X, col.Value.Y, col.Value.Z);
                        //res.PbrMetallicRoughness.RoughnessFactor = col.Value.W;
                        break;
                    case "_SpecularColor":
                    case "_SpecColor":
                        pbr.SpecularFactor = new(col.Value.X, col.Value.Y, col.Value.Z);
                        //res.PbrMetallicRoughness.MetallicFactor = col.Value.W;
                        break;
                    case "_ReflectColor":
                        //res.PbrMetallicRoughness.BaseColorFactor = col.Value;
                        //res.PbrMetallicRoughness.MetallicFactor = 1;
                        pbr.DiffuseFactor = col.Value;
                        break;
                }
            }

            foreach (var flt in mat.m_SavedProperties.m_Floats)
            {
                switch (flt.Key)
                {
                    case "_Glossiness":
                        pbr.GlossinessFactor = flt.Value;
                        break;
                    //case "_Shininess":
                    //    pbr.GlossinessFactor = flt.Value;
                    //    break;
                    case "_Transparency":
                        res.AlphaCutoff = flt.Value;
                        break;
                }
            }
            foreach (var texEnv in mat.m_SavedProperties.m_TexEnvs)
            {
                if (!texEnv.Value.m_Texture.TryGet<Texture2D>(out var image)) //TODO other Texture
                {
                    continue;
                }
                var trans = new TextureTransform()
                {
                    Offset = texEnv.Value.m_Offset,
                    Scale = texEnv.Value.m_Scale
                };
                if (texEnv.Key == "_MainTex")
                {
                    pbr.DiffuseTexture = new()
                    {
                        Index = AddTexture(image),
                        Extensions = new()
                        {
                            {TextureTransform.ExtensionName, trans }
                        }
                    };
                    _root.AddExtensionUsed(TextureTransform.ExtensionName);
                }
                else if (texEnv.Key == "_BumpMap")
                {
                    res.NormalTexture = new()
                    {
                        Index = AddTexture(image, () => {
                            _attachItems.TryAdd(image.Name, new NormalTextureExporter(image));
                        }),
                        Extensions = new()
                        {
                            {TextureTransform.ExtensionName, trans }
                        }
                    };
                    _root.AddExtensionUsed(TextureTransform.ExtensionName);
                }
                else if (texEnv.Key.Contains("Emission"))
                {
                    res.EmissiveTexture = new()
                    {
                        Index = AddTexture(image),
                        Extensions = new()
                        {
                            {TextureTransform.ExtensionName, trans }
                        }
                    };
                    res.EmissiveFactor = null;
                    _root.AddExtensionUsed(TextureTransform.ExtensionName);
                }
                else if (texEnv.Key.Contains("Specular") || texEnv.Key.Contains("Spec"))
                {
                    pbr.SpecularGlossinessTexture = new()
                    {
                        Index = AddTexture(image),
                        Extensions = new()
                        {
                            {TextureTransform.ExtensionName, trans }
                        }
                    };
                    pbr.SpecularFactor = null;
                    _root.AddExtensionUsed(TextureTransform.ExtensionName);
                }
                else if (texEnv.Key.Contains("Normal"))
                {
                    res.NormalTexture = new()
                    {
                        Index = AddTexture(image),
                        Extensions = new()
                        {
                            {TextureTransform.ExtensionName, trans }
                        }
                    };
                    _root.AddExtensionUsed(TextureTransform.ExtensionName);
                }
                
            }
            materialIndex = _root.Add(res);
            _materialItems.Add(mat.Name, materialIndex);
            return materialIndex;
        }

        private int AddTexture(Texture2D image, Action? addFn = null)
        {
            _root.Textures ??= [];
            for (int i = 0; i < _root.Textures.Count; i++)
            {
                if (_root.Textures[i].Name == image.Name)
                {
                    return i;
                }
            }
            addFn?.Invoke();
            return _root.Add(new KhronosExporter.Models.Texture()
            {
                Name = image.m_Name,
                Sampler = _root.Add(new TextureSampler()
                {
                    MinFilter = TextureMipMapFilter.LINEAR_MIPMAP_LINEAR,
                    MagFilter = TextureInterpolationFilter.LINEAR,
                    WrapS = TextureWrapMode.REPEAT,
                    WrapT = TextureWrapMode.REPEAT
                }),
                Source = _root.Add(new Image()
                {
                    Name = image.m_Name,
                    Uri = $"{image.m_Name}.png",
                    MimeType = "image/png"
                })
            });
        }

       

        #region 动画


        /// <summary>
        /// 最后处理所有动画
        /// </summary>
        private void AddAnimation()
        {
            foreach (var item in _animationItems)
            {
                AddAnimation(item);
            }
        }
        private void AddAnimation(AnimationClip animator)
        {
            var res = new Animation()
            {
                Name = animator.m_Name,
            };
            var timeMaps = new Dictionary<string, int>();
            if (animator.m_Legacy)
            {
                foreach (var m_CompressedRotationCurve in animator.m_CompressedRotationCurves)
                {
                    var numKeys = m_CompressedRotationCurve.m_Times.m_NumItems;
                    var data = m_CompressedRotationCurve.m_Times.UnpackInts();
                    var times = new float[numKeys];
                    int t = 0;
                    for (int i = 0; i < numKeys; i++)
                    {
                        t += data[i];
                        times[i] = t * 0.01f / animator.m_SampleRate;
                    }
                    var timeIndex = _root.CreateAccessor("Times_" + m_CompressedRotationCurve.m_Path, times, true);
                    timeMaps.TryAdd(m_CompressedRotationCurve.m_Path, timeIndex);
                    var quats = m_CompressedRotationCurve.m_Values.UnpackQuats();
                    var values = new float[numKeys * 4];
                    for (int i = 0; i < numKeys; i++)
                    {
                        var quat = quats[i];
                        var offset = i * 4;
                        values[offset] = quat.X;
                        values[offset + 1] = -quat.Y;
                        values[offset + 2] = -quat.Z;
                        values[offset + 3] = quat.W;
                    }
                    res.Channels.Add(new()
                    {
                        Sampler = res.Samplers.AddWithIndex(new()
                        {
                            Input = timeIndex,
                            Output = _root.CreateVectorAccessor("RotationCurve", 
                            values, (int)numKeys, true),
                            Interpolation = AnimationInterpolationMode.LINEAR
                        }),
                        Target = new()
                        {
                            Node = 0, // TODO find
                            Path = PropertyPath.Rotation
                        }
                    });
                }
                foreach (var m_RotationCurve in animator.m_RotationCurves)
                {
                    if (!timeMaps.TryGetValue(m_RotationCurve.path, out var timeIndex))
                    {
                        continue;
                    }
                    var values = new float[m_RotationCurve.curve.m_Curve.Count * 4];
                    var offset = 0;
                    foreach (var m_Curve in m_RotationCurve.curve.m_Curve)
                    {
                        values[offset ++] = m_Curve.value.X;
                        values[offset ++] = -m_Curve.value.Y;
                        values[offset ++] = -m_Curve.value.Z;
                        values[offset ++] = m_Curve.value.W;
                    }
                    res.Channels.Add(new()
                    {
                        Sampler = res.Samplers.AddWithIndex(new()
                        {
                            Input = timeIndex,
                            Output = _root.CreateVectorAccessor("RotationCurve", 
                            values, m_RotationCurve.curve.m_Curve.Count, true),
                            Interpolation = AnimationInterpolationMode.LINEAR
                        }),
                        Target = new()
                        {
                            Node = 0, // TODO find
                            Path = PropertyPath.Rotation
                        }
                    });
                }
                foreach (var m_PositionCurve in animator.m_PositionCurves)
                {
                    if (!timeMaps.TryGetValue(m_PositionCurve.path, out var timeIndex))
                    {
                        continue;
                    }
                    var values = new float[m_PositionCurve.curve.m_Curve.Count * 3];
                    var offset = 0;
                    foreach (var m_Curve in m_PositionCurve.curve.m_Curve)
                    {
                        values[offset++] = -m_Curve.value.X;
                        values[offset++] = m_Curve.value.Y;
                        values[offset++] = m_Curve.value.Z;
                    }
                    res.Channels.Add(new()
                    {
                        Sampler = res.Samplers.AddWithIndex(new()
                        {
                            Input = timeIndex,
                            Output = _root.CreateVectorAccessor("PositionCurve", 
                            values, m_PositionCurve.curve.m_Curve.Count, true),
                            Interpolation = AnimationInterpolationMode.LINEAR
                        }),
                        Target = new()
                        {
                            Node = 0, // TODO find
                            Path = PropertyPath.Translation
                        }
                    });
                }
                foreach (var m_ScaleCurve in animator.m_ScaleCurves)
                {
                    if (!timeMaps.TryGetValue(m_ScaleCurve.path, out var timeIndex))
                    {
                        continue;
                    }
                    var values = new float[m_ScaleCurve.curve.m_Curve.Count * 3];
                    var offset = 0;
                    foreach (var m_Curve in m_ScaleCurve.curve.m_Curve)
                    {
                        values[offset++] = m_Curve.value.X;
                        values[offset++] = m_Curve.value.Y;
                        values[offset++] = m_Curve.value.Z;
                    }
                    res.Channels.Add(new()
                    {
                        Sampler = res.Samplers.AddWithIndex(new()
                        {
                            Input = timeIndex,
                            Output = _root.CreateVectorAccessor("ScaleCurve", values, 
                            m_ScaleCurve.curve.m_Curve.Count, true),
                            Interpolation = AnimationInterpolationMode.LINEAR
                        }),
                        Target = new()
                        {
                            Node = 0, // TODO find
                            Path = PropertyPath.Scale
                        }
                    });
                }
                if (animator.m_EulerCurves != null)
                {
                    foreach (var m_EulerCurve in animator.m_EulerCurves)
                    {
                        if (!timeMaps.TryGetValue(m_EulerCurve.path, out var timeIndex))
                        {
                            continue;
                        }
                        var values = new float[m_EulerCurve.curve.m_Curve.Count * 4];
                        var offset = 0;
                        foreach (var m_Curve in m_EulerCurve.curve.m_Curve)
                        {
                            var item = new Vector3(m_Curve.value.X, -m_Curve.value.Y, -m_Curve.value.Z).ToQuaternion();
                            values[offset++] = item.X;
                            values[offset++] = item.Y;
                            values[offset++] = item.Z;
                            values[offset++] = item.W;
                        }
                        res.Channels.Add(new()
                        {
                            Sampler = res.Samplers.AddWithIndex(new()
                            {
                                Input = timeIndex,
                                Output = _root.CreateVectorAccessor("RotationCurve", 
                                values, m_EulerCurve.curve.m_Curve.Count, true),
                                Interpolation = AnimationInterpolationMode.LINEAR
                            }),
                            Target = new()
                            {
                                Node = 0, // TODO find
                                Path = PropertyPath.Rotation
                            }
                        });
                    }
                }
                foreach (var m_FloatCurve in animator.m_FloatCurves)
                {
                    if (m_FloatCurve.classID == ElementIDType.SkinnedMeshRenderer) //BlendShape
                    {
                        var channelName = m_FloatCurve.attribute;
                        int dotPos = channelName.IndexOf('.');
                        if (dotPos >= 0)
                        {
                            channelName = channelName.Substring(dotPos + 1);
                        }

                        if (!timeMaps.TryGetValue(m_FloatCurve.path ?? GetPathByChannelName(channelName), out var timeIndex))
                        {
                            continue;
                        }
                        // TODO 更改通道 channelName;
                        var values = new float[m_FloatCurve.curve.m_Curve.Count];
                        var offset = 0;
                        foreach (var m_Curve in m_FloatCurve.curve.m_Curve)
                        {
                            values[offset++] = m_Curve.value;
                        }
                        res.Channels.Add(new()
                        {
                            Sampler = res.Samplers.AddWithIndex(new()
                            {
                                Input = timeIndex,
                                Output = _root.CreateAccessor("RotationCurve", values, true),
                                Interpolation = AnimationInterpolationMode.LINEAR
                            }),
                            Target = new()
                            {
                                Node = 0, // TODO find
                                Path = PropertyPath.Weights
                            }
                        });
                    }
                }
            } 
            else
            {
                var cachedData = new Dictionary<string, List<float>[]>(); 
                var m_Clip = animator.m_MuscleClip.m_Clip;
                var streamedFrames = m_Clip.m_StreamedClip.ReadData();
                var m_ClipBindingConstant = animator.m_ClipBindingConstant ?? m_Clip.ConvertValueArrayToGenericBinding();
                for (int frameIndex = 1; frameIndex < streamedFrames.Count - 1; frameIndex++)
                {
                    var frame = streamedFrames[frameIndex];
                    var streamedValues = frame.keyList.Select(x => x.value).ToArray();
                    for (int curveIndex = 0; curveIndex < frame.keyList.Count;)
                    {
                        ReadCurveData(cachedData, m_ClipBindingConstant, frame.keyList[curveIndex].index, frame.time, streamedValues, 0, ref curveIndex);
                    }
                }
                var m_DenseClip = m_Clip.m_DenseClip;
                var streamCount = m_Clip.m_StreamedClip.curveCount;
                for (int frameIndex = 0; frameIndex < m_DenseClip.m_FrameCount; frameIndex++)
                {
                    var time = m_DenseClip.m_BeginTime + frameIndex / m_DenseClip.m_SampleRate;
                    var frameOffset = frameIndex * m_DenseClip.m_CurveCount;
                    for (int curveIndex = 0; curveIndex < m_DenseClip.m_CurveCount;)
                    {
                        var index = streamCount + curveIndex;
                        ReadCurveData(cachedData, m_ClipBindingConstant, (int)index, time, m_DenseClip.m_SampleArray, (int)frameOffset, ref curveIndex);
                    }
                }
                if (m_Clip.m_ConstantClip != null)
                {
                    var m_ConstantClip = m_Clip.m_ConstantClip;
                    var denseCount = m_Clip.m_DenseClip.m_CurveCount;
                    var time2 = 0.0f;
                    for (int i = 0; i < 2; i++)
                    {
                        for (int curveIndex = 0; curveIndex < m_ConstantClip.data.Length;)
                        {
                            var index = streamCount + denseCount + curveIndex;
                            ReadCurveData(cachedData, m_ClipBindingConstant, (int)index, time2, m_ConstantClip.data, 0, ref curveIndex);
                        }
                        time2 = animator.m_MuscleClip.m_StopTime;
                    }
                }
                foreach (var track in cachedData)
                {
                    for (int i = 0; i < 10; i += 2)
                    {
                        if (track.Value[i].Count == 0)
                        {
                            continue;
                        }
                        res.Channels.Add(new()
                        {
                            Sampler = res.Samplers.AddWithIndex(new()
                            {
                                Input = _root.CreateAccessor("Times_" + track.Key + i, track.Value[i].ToArray(), true),
                                Output = _root.CreateVectorAccessor("AnyCurve", track.Value[i + 1].ToArray(), track.Value[i].Count),
                                Interpolation = AnimationInterpolationMode.LINEAR
                            }),
                            Target = new()
                            {
                                Node = 0, // TODO find
                                Path = i switch
                                {
                                    0 => PropertyPath.Weights,
                                    2 => PropertyPath.Translation,
                                    4 or 8 => PropertyPath.Rotation,
                                    6 => PropertyPath.Scale,
                                    _ => throw new NotImplementedException(),
                                }
                            }
                        });
                    }
                }
            } 
            _root.Add(res);
        }

        private void ReadCurveData(Dictionary<string, List<float>[]> cachedData, 
            AnimationClipBindingConstant m_ClipBindingConstant, 
            int index, float time, 
            float[] data, 
            int offset, ref int curveIndex)
        {
            var binding = m_ClipBindingConstant.FindBinding(index);
            if (binding.typeID == ElementIDType.SkinnedMeshRenderer) //BlendShape
            {
                var channelName = GetChannelNameFromHash(binding.attribute);
                if (string.IsNullOrEmpty(channelName))
                {
                    curveIndex++;
                    return;
                }
                int dotPos = channelName.IndexOf('.');
                if (dotPos >= 0)
                {
                    channelName = channelName.Substring(dotPos + 1);
                }

                var bPath = GetPathFromHash(binding.path);
                if (string.IsNullOrEmpty(bPath))
                {
                    bPath = GetPathByChannelName(channelName);
                }
                if (!cachedData.TryGetValue(bPath, out var track))
                {
                    track = CreateArray(10);
                    cachedData.Add(bPath, track);
                }
                // channelName;
                // BlendShape
                track[0].Add(time);
                track[1].Add(data[curveIndex++ + offset]);
            }
            else if (binding.typeID == ElementIDType.Transform)
            {
                var path = GetPathFromHash(binding.path);
                if (!cachedData.TryGetValue(path, out var track))
                {
                    track = CreateArray(10);
                    cachedData.Add(path, track);
                }

                switch (binding.attribute)
                {
                    case 1:
                        //Translations
                        track[2].Add(time);
                        track[3].AddRange(
                            -data[curveIndex++ + offset],
                            data[curveIndex++ + offset],
                            data[curveIndex++ + offset]
                            );
                        break;
                    case 2:
                        // Rotations
                        track[4].Add(time);
                        track[5].AddRange(
                           data[curveIndex++ + offset],
                            -data[curveIndex++ + offset],
                            -data[curveIndex++ + offset],
                            data[curveIndex++ + offset]
                        );
                        break;
                    case 3:
                        // Scalings
                        track[6].Add(time);
                        track[7].AddRange(
                          data[curveIndex++ + offset],
                            data[curveIndex++ + offset],
                            data[curveIndex++ + offset]
                        );
                        break;
                    case 4:
                        // Rotations
                        track[8].Add(time);
                        track[9].AddRange(
                          new Vector3(
                              data[curveIndex++ + offset],
                            -data[curveIndex++ + offset],
                            -data[curveIndex++ + offset]).ToQuaternion().AsArray()
                        );
                        break;
                    default:
                        curveIndex++;
                        break;
                }
            }
            else
            {
                curveIndex++;
            }
        }


        private static List<float>[] CreateArray(int length)
        {
            var res = new List<float>[length];
            for (int i = 0; i < length; i++) 
            {
                res[i] = [];
            }
            return res;
        }

        private string? GetChannelNameFromHash(uint attribute)
        {
            if (_morphChannelNames.TryGetValue(attribute, out var name))
            {
                return name;
            }
            else
            {
                return null;
            }
        }


        private void CreateBonePathHash(Transform m_Transform)
        {
            var name = FbxExporter.GetTransformPathByFather(m_Transform);
            var bytes = Encoding.UTF8.GetBytes(name);
            _bonePathHash[Crc32.Compute(bytes)] = name;
            int index;
            while ((index = name.IndexOf('/')) >= 0)
            {
                name = name[(index + 1)..];
                bytes = Encoding.UTF8.GetBytes(name);
                _bonePathHash[Crc32.Compute(bytes)] = name;
            }
            foreach (var pptr in m_Transform.m_Children)
            {
                if (pptr.TryGet(out var child))
                {
                    CreateBonePathHash(child);
                }
            }
        }

        private string GetPathFromHash(uint hash)
        {
            _bonePathHash.TryGetValue(hash, out var boneName);
            if (string.IsNullOrEmpty(boneName))
            {
                boneName = _avatar?.FindBonePath(hash);
            }
            if (string.IsNullOrEmpty(boneName))
            {
                boneName = "unknown " + hash;
            }
            return boneName;
        }

        private string? GetPathByChannelName(string channelName)
        {
            if (_channelPathItems.TryGetValue(channelName, out var item)) 
            {
                return item;
            }
            return null;
        }
        #endregion
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (IsEmpty || !LocationStorage.TryCreate(fileName, IsBinaryFile ? ".glb" : ".gltf", mode, out fileName))
            {
                return;
            }
            if (_root.Materials?.Count == 0)
            {
                _root.Add(new Material()
                {
                    Name = "Material",
                    AlphaMode = AlphaMode.OPAQUE,
                    DoubleSided = true,
                    PbrMetallicRoughness = new()
                    {
                        RoughnessFactor = .5f,
                        BaseColorFactor = new(0.8f, 0.8f, 0.8f, 1f)
                    }
                });
            }
            AddAnimation();
            var folder = Path.GetDirectoryName(fileName);
            foreach (var item in _attachItems)
            {
                item.Value.SaveAs(Path.Combine(folder, item.Key), mode);
            }
            _root.FileName = fileName;
            using var fs = File.Create(fileName);
            if (IsBinaryFile)
            {
                new GlbWriter().Write(_root, fs);
            } else
            {
                new GltfWriter().Write(_root, fs);
            }
        }

        public void Dispose()
        {
            _attachItems.Clear();
            _root.Dispose();
        }
    }
}
