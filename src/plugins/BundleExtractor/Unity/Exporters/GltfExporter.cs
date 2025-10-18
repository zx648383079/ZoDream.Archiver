using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;
using ZoDream.AutodeskExporter;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.KhronosExporter;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Collections;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Numerics;
using ZoDream.Shared.Storage;
using Animation = ZoDream.KhronosExporter.Models.Animation;
using Material = ZoDream.KhronosExporter.Models.Material;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Mesh = ZoDream.KhronosExporter.Models.Mesh;
using Node = ZoDream.KhronosExporter.Models.Node;
using UnityAnimation = UnityEngine.Animation;
using UnityMaterial = UnityEngine.Material;
using UnityMesh = UnityEngine.Mesh;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    /// <summary>
    /// unity 使用的时 y up 坐标 gltf 使用 z up 坐标
    /// </summary>
    internal class GltfExporter : IMultipartBuilder
    {
        public GltfExporter(ISerializedFile resource)
        {
            _resource = resource;
            _root = new();
            _root.Add(new Scene());
        }

        private readonly ISerializedFile _resource;
        private bool IsBinaryFile => _resource.Container?.Options?.ModelFormat == "glb";
        private readonly ModelSource _root;
        private readonly Dictionary<string, IBundleExporter> _attachItems = [];
        /// <summary>
        /// mesh to node
        /// </summary>
        private readonly Dictionary<long, int> _nodeItems = [];
        /// <summary>
        /// frame to node
        /// </summary>
        private readonly Dictionary<string, int> _pathNodeItems = [];
        private readonly Dictionary<string, int> _materialItems = [];
        private readonly HashSet<AnimationClip> _animationItems = [];
        private readonly Dictionary<uint, string> _morphChannelNames = [];
        /// <summary>
        /// unity 内部的 crc 名称绑定
        /// </summary>
        private readonly Dictionary<uint, string> _bonePathHash = [];
        private readonly Dictionary<string, int> _channelPathItems = [];
        /// <summary>
        /// transform 转 node 
        /// </summary>
        private readonly Dictionary<Transform, int> _transformItems = [];

        private Avatar? _avatar;
        private long _entryFileId;

        public bool IsEmpty => _root.Nodes.Count == 0;
        public string FileName { get; private set; } = string.Empty;

        public IFilePath SourcePath => _resource.FullPath;

        public void Append(int entryId)
        {
            var obj = _resource[entryId];
            var fileId = _resource.Get(entryId).FileID;
            _entryFileId = fileId;
            if (string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(obj.Name))
            {
                FileName = obj.Name;
            }
            if (_nodeItems.ContainsKey(fileId))
            {
                return;
            }
            switch (obj)
            {
                case GameObject g:
                    Append(g);
                    break;
                case Animator a:
                    Append(a);
                    break;
                case AnimationClip c:
                    Append(c);
                    break;
            }
        }
        public void Append(GameObject obj)
        {
            if (GameObjectConverter.TryGet<Animator>(obj, out var animator))
            {
                AddAnimator(animator);
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
            //if (string.IsNullOrEmpty(FileName))
            //{
            //    FileName = mesh.m_Name;
            //}
            //AddNode(mesh, _root.Scene, -1);
        }

        public void Append(Animator animator)
        {
            AddAnimator(animator);
        }

        public void Append(AnimationClip animator)
        {
            _animationItems.Add(animator);
        }


        private void AddAnimator(Animator animator)
        {
            if (animator.Avatar.TryGet(out var m_Avatar))
            {
                _avatar = m_Avatar;
            }
            animator.GameObject.IsExclude = true;
            if (animator.GameObject.TryGet(out var game))
            {
                AddGame(game);
            }
        }
        private void AddGame(GameObject game, bool hasTransformHierarchy = true)
        {
            if (!GameObjectConverter.TryGet<Transform>(game, out var transform))
            {
                return;
            }
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

            AddMeshRenderer(transform);
            CreateBonePathHash(transform);
        }

        private void AddMeshRenderer(Transform transform, int meshParent = -1)
        {
            transform.GameObject.IsExclude = true;
            if (transform.GameObject.TryGet(out var game))
            {
                foreach (var item in GameObjectConverter.ForEach<Component>(game))
                {
                    switch (item)
                    {
                        case MeshRenderer renderer:
                            meshParent = AddMeshRenderer(renderer, meshParent);
                            if (meshParent >= 0)
                            {
                                _transformItems.TryAdd(transform, meshParent);
                            }
                            break;
                        case SkinnedMeshRenderer renderer:
                            var skinIndex = AddMeshRenderer(renderer);
                            if (skinIndex >= 0)
                            {
                                _transformItems.TryAdd(transform, skinIndex);
                            }
                            break;
                        case UnityAnimation animation:
                            foreach (var pptr in animation.Clips)
                            {
                                if (pptr.TryGet(out var animationClip))
                                {
                                    //if (!boundAnimationPathDic.ContainsKey(animationClip))
                                    //{
                                    //    boundAnimationPathDic.Add(animationClip, GetTransformPath(m_Transform));
                                    //}
                                    _animationItems.Add(animationClip);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            foreach (var pptr in transform.Children)
            {
                pptr.IsExclude = true;
                if (pptr.TryGet(out var child))
                {
                    AddMeshRenderer(child, meshParent);
                }
            }
        }

        private int AddTransform(Transform trans)
        {
            if (_transformItems.TryGetValue(trans, out var nodeIndex))
            {
                return nodeIndex;
            }
            trans.GameObject.TryGet(out var m_GameObject);
            nodeIndex = FindNode(m_GameObject?.Name, -1);
            _transformItems.Add(trans, nodeIndex);
            UpdateNode(nodeIndex, trans.LocalPosition, trans.LocalRotation, trans.LocalScale);
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
            foreach (var pptr in trans.Children)
            {
                if (pptr.TryGet(out var child))
                {
                    AddTransforms(child, nodeIndex);
                }
            }
        }

        private int AddMeshRenderer(Renderer meshR, int meshParent = -1)
        {
            var meshPtr = GetMesh(meshR);
            if (meshPtr == null || !meshPtr.TryGet(out var mesh))
            {
                return -1;
            }
            var nodeIndex = AddNode(meshPtr.PathID, mesh, 0, meshParent, meshR);
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                //Bone
                /*
                 * 0 - None
                 * 1 - m_Bones
                 * 2 - m_BoneNameHashes
                 */
                var boneType = 0;
                if (sMesh.Bones.Length > 0)
                {
                    if (sMesh.Bones.Length == mesh.BindPose.Length)
                    {
                        var verifiedBoneCount = sMesh.Bones.Count(x => x.TryGet(out _));
                        if (verifiedBoneCount > 0)
                        {
                            boneType = 1;
                        }
                        if (verifiedBoneCount != sMesh.Bones.Length)
                        {
                            //尝试使用m_BoneNameHashes 4.3 and up
                            if (mesh.BindPose.Length > 0 && (mesh.BindPose.Length == mesh.BoneNameHashes?.Length))
                            {
                                //有效bone数量是否大于SkinnedMeshRenderer
                                var verifiedBoneCount2 = mesh.BoneNameHashes.Count(x => GetPathFromHash(x) != null);
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
                    if (mesh.BindPose.Length > 0 && (mesh.BindPose.Length == mesh.BoneNameHashes?.Length))
                    {
                        var verifiedBoneCount = mesh.BoneNameHashes.Count(x => GetPathFromHash(x) != null);
                        if (verifiedBoneCount > 0)
                        {
                            boneType = 2;
                        }
                    }
                }

                if (boneType == 1)
                {
                    var boneCount = sMesh.Bones.Length;
                    var joinItems = new int[boneCount];
                    var matrixItems = new Matrix4x4[boneCount];
                    for (int i = 0; i < boneCount; i++)
                    {
                        if (sMesh.Bones[i].TryGet(out var m_Transform))
                        {
                            joinItems[i] = FindNodeByTransform(m_Transform, nodeIndex);
                        }
                        var convert = Matrix4x4.CreateScale(new Vector3(-1, 1, 1));
                        matrixItems[i] = convert * mesh.BindPose[i] * convert;
                    }
                    _root.Nodes[nodeIndex].Skin = _root.Add(new Skin()
                    {
                        InverseBindMatrices = _root.CreateAccessor($"skins_{nodeIndex}", matrixItems),
                        Joints = [.. joinItems.Distinct()],
                        Skeleton = nodeIndex,
                    });
                }
                else if (boneType == 2)
                {
                    var boneCount = mesh.BindPose.Length;
                    var joinItems = new int[boneCount];
                    var matrixItems = new Matrix4x4[boneCount];
                    for (int i = 0; i < boneCount; i++)
                    {
                        var bone = new FbxImportedBone();
                        var boneHash = mesh.BoneNameHashes[i];
                        var path = GetPathFromHash(boneHash);
                        joinItems[i] = FindNode(path, nodeIndex);
                        var convert = Matrix4x4.CreateScale(new Vector3(-1, 1, 1));
                        matrixItems[i] = convert * mesh.BindPose[i] * convert;
                    }
                    _root.Nodes[nodeIndex].Skin = _root.Add(new Skin()
                    {
                        InverseBindMatrices = _root.CreateAccessor($"skins_{nodeIndex}", matrixItems),
                        Joints = [.. joinItems.Distinct()],
                        Skeleton = nodeIndex,
                    });
                }

                //Morphs
                if (mesh.Shapes?.Channels?.Length > 0)
                {
                    if (meshR.GameObject.TryGet(out var game) &&
                        GameObjectConverter.TryGet<Transform>(game, out var transform))
                    {
                        _channelPathItems[mesh.Name] = FindNodeByTransform(transform);
                    }
                    for (int i = 0; i < mesh.Shapes.Channels.Length; i++)
                    {
                        var shapeChannel = mesh.Shapes.Channels[i];
                        var blendShapeName = "blendShape." + shapeChannel.Name;
                        var bytes = Encoding.UTF8.GetBytes(blendShapeName);
                        _morphChannelNames[Crc32.HashToUInt32(bytes)] = blendShapeName;

                        var node = _root.Meshes[FindNode(shapeChannel.Name, nodeIndex)];
                        node.Weights ??= [];
                        var frameEnd = shapeChannel.FrameIndex + shapeChannel.FrameCount;
                        for (int frameIdx = shapeChannel.FrameIndex; frameIdx < frameEnd; frameIdx++)
                        {
                            node.Weights.Add(mesh.Shapes.FullWeights[frameIdx]);
                            var shape = mesh.Shapes.Shapes[frameIdx];

                            var vertexEnd = shape.FirstVertex + shape.VertexCount;
                            var vertItems = new List<float>((int)shape.VertexCount * 3);
                            var normalItems = new List<float>((int)shape.VertexCount * 3);
                            var tangentItems = new List<float>((int)shape.VertexCount * 4);
                            for (uint j = shape.FirstVertex; j < vertexEnd; j++)
                            {
                                var morphVertex = mesh.Shapes.Vertices[(int)j];
                                
                                var morphPos = morphVertex.Vertex;
                                vertItems.AddRange(-morphPos.X, morphPos.Y, morphPos.Z);
                                if (shape.HasNormals)
                                {
                                    var morphNormal = morphVertex.Normal;
                                    normalItems.AddRange(-morphNormal.X, morphNormal.Y, morphNormal.Z);
                                }
                                if (shape.HasTangents)
                                {
                                    var morphTangent = morphVertex.Tangent;
                                    tangentItems.AddRange(-morphTangent.X, morphTangent.Y, morphTangent.Z, 0);
                                }
                            }
                            var target = new Dictionary<string, int>()
                            {
                                {"POSITION", _root.CreateVectorAccessor("blendShapeName", vertItems.ToArray(), (int)shape.VertexCount) }
                            };
                            if (shape.HasNormals)
                            {
                                target.Add("NORMAL", _root.CreateVectorAccessor("blendShapeName", normalItems.ToArray(), (int)shape.VertexCount));
                            }
                            if (shape.HasTangents)
                            {
                                target.Add("TANGENT", _root.CreateVectorAccessor("blendShapeName", tangentItems.ToArray(), (int)shape.VertexCount));
                            }
                            // TODO 
                            (node.Primitives[0].Targets ??= []).Add(target);
                        }
                    }
                }
            }
            return nodeIndex;
        }

        private int FindNode(string path, int parent)
        {
            var routes = path.Split('/');
            for (var i = routes.Length - 1; i >= 0; i--)
            {
                var j = FindNode(routes, i, parent);
                if (j >= 0)
                {
                    return j;
                }
            }
            return parent;
        }

        private int FindNode(string path)
        {
            if (_pathNodeItems.TryGetValue(path, out var nodexIndex))
            {
                return nodexIndex;
            }
            var routes = path.Split('/');
            for (var i = 0; i < _root.Nodes.Count; i++)
            {
                var j =  FindNode(routes, 0, i);
                if (j >= 0)
                {
                    _pathNodeItems.TryAdd(path, j);
                    return j;
                }
            }
            return -1;
        }

        private int FindNode(string[] routes, int routeIndex, int parentNode)
        {
            var parent = _root.Nodes[parentNode];
            if (!parent.Name.Equals(routes[routeIndex], StringComparison.Ordinal))
            {
                return -1;
            }
            if (routeIndex == routes.Length - 1)
            {
                return parentNode;
            }
            if (parent.Children is null || parent.Children.Count == 0)
            {
                return -1;
            }
            foreach (var i in parent.Children)
            {
                var j = FindNode(routes, routeIndex + 1, i);
                if (j >= 0)
                {
                    return j;
                }
            }
            return -1;
        }

        private static IPPtr<UnityMesh>? GetMesh(Renderer meshR)
        {
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                return sMesh.Mesh;
            }
            else if (true == meshR.GameObject?.TryGet(out var game) &&
                GameObjectConverter.TryGet<MeshFilter>(game, out var filter))
            {
                return filter.Mesh;
            }

            return null;
        }

        private int FindNodeByTransform(Transform transform, int parentNode = -1)
        {
            if (_transformItems.TryGetValue(transform, out var item))
            {
                return item;
            }
            return parentNode;
        }
        private void AddNode(Transform trans, int sceneIndex, int parentIndex)
        {
            var nodeIndex = parentIndex;
            if (!trans.GameObject.TryGet(out var game) && 
                GameObjectConverter.TryGet<MeshRenderer>(game, out var meshR))
            {
                var res = AddNode(meshR, sceneIndex, parentIndex);
                // var skin = AddNode(obj.m_SkinnedMeshRenderer, sceneIndex);
                if (res >= 0)
                {
                    UpdateNode(res, trans.LocalPosition, trans.LocalRotation, trans.LocalScale);
                    nodeIndex = res;
                }
            }
            foreach (var pptr in trans.Children)
            {
                if (pptr.TryGet(out var t))
                {
                    AddNode(t, sceneIndex, nodeIndex);
                }
            }
        }

        private int AddNode(Renderer renderer, int sceneIndex, int parentIndex)
        {
            var meshPtr = GetMesh(renderer);
            if (meshPtr == null || !meshPtr.TryGet(out var mesh))
            {
                return -1;
            }
            return AddNode(meshPtr.PathID, mesh, sceneIndex, parentIndex);
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

        private int AddNode(long meshId, UnityMesh mesh, int sceneIndex, int parentIndex, 
            Renderer? meshR = null)
        {
            if (string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(mesh.Name))
            {
                FileName = mesh.Name + _entryFileId.ToString()[^3..];
            }
            if (_nodeItems.TryGetValue(meshId, out var cacheIndex))
            {
                BindNode(parentIndex, cacheIndex);
                return cacheIndex;
            }
            
            #region 转换 Mesh
            var psItems = new List<MeshPrimitive>();
            var hasUv = mesh.UV0?.Length > 0;
            var vStep = ComputeStep(mesh.Vertices.Length, mesh.VertexCount, 3, 4);
            var uStep = ComputeStep(mesh.UV0?.Length??0, mesh.VertexCount, 4, 2, 3);
            var nStep = ComputeStep(mesh.Normals?.Length??0, mesh.VertexCount, 3, 4);
            var cStep = ComputeStep(mesh.Colors?.Length??0, mesh.VertexCount, 3, 4);
            var hasColor = mesh.Colors?.Length > 0;
            var hasNormal = mesh.Normals?.Length > 0;
            int firstSubMesh = 0;
            if (meshR is not null && meshR.StaticBatchInfo.SubMeshCount > 0)
            {
                firstSubMesh = meshR.StaticBatchInfo.FirstSubMesh;
            }
            else if (meshR is not null && meshR.SubsetIndices?.Length > 0)
            {
                firstSubMesh = (int)meshR.SubsetIndices.Min(x => x);
            }
            int sum = 0;
            for (var i = 0; i < mesh.SubMeshes.Length; i++)
            {
                var indexCount = (int)mesh.SubMeshes[i].IndexCount;
                var end = sum + indexCount / 3;
                int? materialIndex = null;
                var mi = i - firstSubMesh;
                if (meshR?.Materials is not null && mi >= 0 && mi < meshR.Materials.Length)
                {
                    if (meshR.Materials[mi].TryGet(out var m_Material))
                    {
                        materialIndex = AddMaterial(m_Material);
                    }
                }

                var positionItems = new List<float>();
                var normalItems = new List<float>();
                var colorItems = new List<float>();
                var uvItems = CreateArray(8);
                var indicesItems = new List<int>();
                var faceVertexCache = new Dictionary<int, int>();
                var faceVertexCount = 0;

                for (int f = sum; f < end; f++)
                {
                    var v1 = (int)mesh.Indices[f * 3 + 2];
                    var v2 = (int)mesh.Indices[f * 3 + 1];
                    var v3 = (int)mesh.Indices[f * 3];

                    var p1 = new Vector3(-mesh.Vertices[v1 * vStep],
                            mesh.Vertices[v1 * vStep + 1],
                            mesh.Vertices[v1 * vStep + 2]);
                    var p2 = new Vector3(-mesh.Vertices[v2 * vStep],
                            mesh.Vertices[v2 * vStep + 1],
                            mesh.Vertices[v2 * vStep + 2]);
                    var p3 = new Vector3(-mesh.Vertices[v3 * vStep],
                            mesh.Vertices[v3 * vStep + 1],
                            mesh.Vertices[v3 * vStep + 2]);

                    var n1 = Vector3.Zero;
                    if (hasNormal)
                    {
                        n1 = new Vector3(-mesh.Normals[v1 * nStep],
                            mesh.Normals[v1 * nStep + 1],
                            mesh.Normals[v1 * nStep + 2]);
                    }

                    if (!faceVertexCache.ContainsKey(v1))
                    {
                        faceVertexCache.Add(v1, faceVertexCount++);
                        positionItems.AddRange(p1.AsArray());
                        if (hasNormal)
                        {
                            normalItems.AddRange(n1.AsArray());
                        }
                        if (hasColor) 
                        {
                            colorItems.AddRange(mesh.Colors[v1 * cStep],
                                mesh.Colors[v1 * cStep + 1],
                                mesh.Colors[v1 * cStep + 2],
                                nStep > 3 ? mesh.Colors[v1 * cStep + 3] : 1f
                            );
                        }
                       

                        if (hasUv)
                        {
                            for (int ui = 0; ui < uvItems.Length; ui++)
                            {
                                var uv = MeshConverter.GetUV(mesh, ui);
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
                        if (hasNormal)
                        {
                            normalItems.AddRange(
                                -mesh.Normals[v2 * nStep],
                                mesh.Normals[v2 * nStep + 1],
                                mesh.Normals[v2 * nStep + 2]
                            );
                        }
                       
                        if (hasColor)
                        {
                            colorItems.AddRange(mesh.Colors[v2 * cStep],
                                mesh.Colors[v2 * cStep + 1],
                                mesh.Colors[v2 * cStep + 2],
                                nStep > 3 ? mesh.Colors[v2 * cStep + 3] : 1f
                            );
                        }

                        if (hasUv)
                        {
                            for (int ui = 0; ui < uvItems.Length; ui++)
                            {
                                var uv = MeshConverter.GetUV(mesh, ui);
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
                        if (hasNormal)
                        {
                            normalItems.AddRange(
                                -mesh.Normals[v3 * nStep],
                                mesh.Normals[v3 * nStep + 1],
                                mesh.Normals[v3 * nStep + 2]
                             );
                        }
                    
                        if (hasColor)
                        {
                            colorItems.AddRange(mesh.Colors[v3 * cStep],
                                mesh.Colors[v3 * cStep + 1],
                                mesh.Colors[v3 * cStep + 2],
                                nStep > 3 ? mesh.Colors[v3 * cStep + 3] : 1f
                            );
                        }

                        if (hasUv)
                        {
                            for (int ui = 0; ui < uvItems.Length; ui++)
                            {
                                var uv = MeshConverter.GetUV(mesh, ui);
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

                    if (hasNormal && ObjReader.CheckWindingCorrect(p1, p2, p3, n1))
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
                    Indices = _root.CreateIndicesAccessor($"{mesh.Name}_{i}_indices"),
                    Material = materialIndex
                };
                _root.AddAccessorBuffer(ps.Indices, indicesItems.ToArray());
                ps.Attributes.Add("POSITION",
                    _root.CreateVectorAccessor($"{mesh.Name}_{i}_positions",
                    positionItems.ToArray(), positionItems.Count / 3));
                if (normalItems.Count > 0)
                {
                    ps.Attributes.Add("NORMAL",
                    _root.CreateVectorAccessor($"{mesh.Name}_{i}_normals",
                        normalItems.ToArray(), normalItems.Count / 3));
                }
                for (int j = 0; j < uvItems.Length; j++)
                {
                    if (uvItems[j].Count > 0)
                    {
                        ps.Attributes.Add($"TEXCOORD_{j}",
                            _root.CreateVectorAccessor($"{mesh.Name}_{i}_{j}_texcoords",
                            uvItems[j].ToArray(), uvItems[j].Count / 2));
                    }
                }
                if (colorItems.Count > 0)
                {
                    ps.Attributes.Add("COLOR_0",
                    _root.CreateVectorAccessor($"{mesh.Name}_{i}_color",
                        colorItems.ToArray(), colorItems.Count / 4));
                }
                psItems.Add(ps);
                sum = end;
            }

            
            #endregion


            var meshIndex = _root.Meshes.AddWithIndex(new Mesh()
            {
                Name = mesh.Name,
                Primitives = psItems
            });
            var nodeIndex = _root.Nodes.AddWithIndex(new()
            {
                Name = mesh.Name,
                Mesh = meshIndex
            });
            _nodeItems.Add(meshId, nodeIndex);
            BindNode(parentIndex, nodeIndex);
            _root.Scenes[sceneIndex].Nodes.Add(nodeIndex);
            return nodeIndex;
        }

        /// <summary>
        /// 绑定 node 的上下级关系 
        /// </summary>
        /// <param name="parentIndex"></param>
        /// <param name="nodeIndex"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void BindNode(int parentIndex, int nodeIndex)
        {
            if (parentIndex >= 0)
            {
                (_root.Nodes[parentIndex].Children ??= []).Add(nodeIndex);
            } else
            {
            }
        }

        private int AddNode(Skeleton mesh, int sceneIndex, int parentIndex)
        {
            var nodeTree = new Dictionary<int, List<int>>();
            for (int i = 0; i < mesh.Node.Length; i++)
            {
                var item = mesh.Node[i];
                var pIndex = Array.IndexOf(mesh.ID, item.ParentId);
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
            var axes = mesh.AxesArray[mesh.Node[index].AxesId];
            if (axes is not null)
            {
                ps.Attributes.Add("POSITION", _root.CreateVectorAccessor("skeleton_vectors", axes.PreQ.AsArray(), 4));
                ps.Attributes.Add("JOINTS_0", _root.CreateVectorAccessor("skeleton_vectors", axes.PostQ.AsArray(), 4));
                ps.Attributes.Add("WEIGHTS_0", _root.CreateVectorAccessor("skeleton_vectors",
                    axes.Sgn is Vector4 o ? o.AsArray() : [..((Vector3)axes.Sgn).AsArray(), axes.Length], 
                4));
            }
            
            var meshIndex = _root.Meshes.AddWithIndex(new Mesh()
            {
                Name = $"skeleton_{mesh.ID[index]}",
                Primitives = {
                    ps
                }
            });
            var nodeIndex = _root.Nodes.AddWithIndex(new()
            {
                Name = $"skeleton_{mesh.ID[index]}",
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
                Name = mat.Name,
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
            foreach (var col in mat.SavedProperties.Colors)
            {
                switch (col.Key)
                {
                    case "_BaseColor":
                    case "_Color":
                         pbr.DiffuseFactor = col.Value;
                        break;
                    case "_SColor":
                        res.PbrMetallicRoughness ??= new();
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

            foreach (var flt in mat.SavedProperties.Floats)
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
            foreach (var texEnv in mat.SavedProperties.TexEnvs)
            {
                if (!texEnv.Value.Texture.TryGet(out var t) || t is not Texture2D image) //TODO other Texture
                {
                    continue;
                }
                var trans = new TextureTransform()
                {
                    Offset = texEnv.Value.Offset,
                    Scale = texEnv.Value.Scale
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
                            _attachItems.TryAdd(image.Name, new NormalTextureExporter(image, _resource));
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
                Name = image.Name,
                Sampler = _root.Add(new TextureSampler()
                {
                    MinFilter = TextureMipMapFilter.LINEAR_MIPMAP_LINEAR,
                    MagFilter = TextureInterpolationFilter.LINEAR,
                    WrapS = TextureWrapMode.REPEAT,
                    WrapT = TextureWrapMode.REPEAT
                }),
                Source = _root.Add(new Image()
                {
                    Name = image.Name,
                    Uri = $"{image.Name}.png",
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
                Name = animator.Name,
            };
            var timeMaps = new Dictionary<int, int>();
            if (animator.Legacy)
            {
                foreach (var m_CompressedRotationCurve in animator.CompressedRotationCurves)
                {
                    var numKeys = m_CompressedRotationCurve.Times.NumItems;
                    var data = PackedIntVectorConverter.UnpackInts(m_CompressedRotationCurve.Times);
                    var times = new float[numKeys];
                    int t = 0;
                    for (int i = 0; i < numKeys; i++)
                    {
                        t += data[i];
                        times[i] = t * 0.01f / animator.SampleRate;
                    }
                    var timeIndex = _root.CreateAccessor("Times_" + m_CompressedRotationCurve.Path, times, true);
                    timeMaps.TryAdd(FindNode(m_CompressedRotationCurve.Path), timeIndex);
                    var quats = PackedQuatVectorConverter.UnpackQuats(m_CompressedRotationCurve.Values);
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
                foreach (var m_RotationCurve in animator.RotationCurves)
                {
                    if (!timeMaps.TryGetValue(FindNode(m_RotationCurve.Path), out var timeIndex))
                    {
                        continue;
                    }
                    var values = new float[m_RotationCurve.Curve.Curve.Length * 4];
                    var offset = 0;
                    foreach (var m_Curve in m_RotationCurve.Curve.Curve)
                    {
                        values[offset ++] = m_Curve.Value.X;
                        values[offset ++] = -m_Curve.Value.Y;
                        values[offset ++] = -m_Curve.Value.Z;
                        values[offset ++] = m_Curve.Value.W;
                    }
                    res.Channels.Add(new()
                    {
                        Sampler = res.Samplers.AddWithIndex(new()
                        {
                            Input = timeIndex,
                            Output = _root.CreateVectorAccessor("RotationCurve", 
                            values, m_RotationCurve.Curve.Curve.Length, true),
                            Interpolation = AnimationInterpolationMode.LINEAR
                        }),
                        Target = new()
                        {
                            Node = 0, // TODO find
                            Path = PropertyPath.Rotation
                        }
                    });
                }
                foreach (var m_PositionCurve in animator.PositionCurves)
                {
                    if (!timeMaps.TryGetValue(FindNode(m_PositionCurve.Path), out var timeIndex))
                    {
                        continue;
                    }
                    var values = new float[m_PositionCurve.Curve.Curve.Length * 3];
                    var offset = 0;
                    foreach (var m_Curve in m_PositionCurve.Curve.Curve)
                    {
                        values[offset++] = -m_Curve.Value.X;
                        values[offset++] = m_Curve.Value.Y;
                        values[offset++] = m_Curve.Value.Z;
                    }
                    res.Channels.Add(new()
                    {
                        Sampler = res.Samplers.AddWithIndex(new()
                        {
                            Input = timeIndex,
                            Output = _root.CreateVectorAccessor("PositionCurve", 
                            values, m_PositionCurve.Curve.Curve.Length, true),
                            Interpolation = AnimationInterpolationMode.LINEAR
                        }),
                        Target = new()
                        {
                            Node = 0, // TODO find
                            Path = PropertyPath.Translation
                        }
                    });
                }
                foreach (var m_ScaleCurve in animator.ScaleCurves)
                {
                    if (!timeMaps.TryGetValue(FindNode(m_ScaleCurve.Path), out var timeIndex))
                    {
                        continue;
                    }
                    var values = new float[m_ScaleCurve.Curve.Curve.Length * 3];
                    var offset = 0;
                    foreach (var m_Curve in m_ScaleCurve.Curve.Curve)
                    {
                        values[offset++] = m_Curve.Value.X;
                        values[offset++] = m_Curve.Value.Y;
                        values[offset++] = m_Curve.Value.Z;
                    }
                    res.Channels.Add(new()
                    {
                        Sampler = res.Samplers.AddWithIndex(new()
                        {
                            Input = timeIndex,
                            Output = _root.CreateVectorAccessor("ScaleCurve", values, 
                            m_ScaleCurve.Curve.Curve.Length, true),
                            Interpolation = AnimationInterpolationMode.LINEAR
                        }),
                        Target = new()
                        {
                            Node = 0, // TODO find
                            Path = PropertyPath.Scale
                        }
                    });
                }
                if (animator.EulerCurves != null)
                {
                    foreach (var m_EulerCurve in animator.EulerCurves)
                    {
                        if (!timeMaps.TryGetValue(FindNode(m_EulerCurve.Path), out var timeIndex))
                        {
                            continue;
                        }
                        var values = new float[m_EulerCurve.Curve.Curve.Length * 4];
                        var offset = 0;
                        foreach (var m_Curve in m_EulerCurve.Curve.Curve)
                        {
                            var item = new Vector3(m_Curve.Value.X, -m_Curve.Value.Y, -m_Curve.Value.Z).ToQuaternion();
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
                                values, m_EulerCurve.Curve.Curve.Length, true),
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
                foreach (var m_FloatCurve in animator.FloatCurves)
                {
                    if (m_FloatCurve.ClassID == NativeClassID.SkinnedMeshRenderer) //BlendShape
                    {
                        var channelName = m_FloatCurve.Attribute;
                        int dotPos = channelName.IndexOf('.');
                        if (dotPos >= 0)
                        {
                            channelName = channelName.Substring(dotPos + 1);
                        }

                        if (!timeMaps.TryGetValue(!string.IsNullOrEmpty(m_FloatCurve.Path) ? FindNode(m_FloatCurve.Path) : GetPathByChannelName(channelName), out var timeIndex))
                        {
                            continue;
                        }
                        // TODO 更改通道 channelName;
                        var values = new float[m_FloatCurve.Curve.Curve.Length];
                        var offset = 0;
                        foreach (var m_Curve in m_FloatCurve.Curve.Curve)
                        {
                            values[offset++] = m_Curve.Value;
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
                var cachedData = new Dictionary<int, List<float>[]>(); 
                var m_Clip = animator.MuscleClip.Clip;
                var streamedFrames = m_Clip.StreamedClip.Data;
                var m_ClipBindingConstant = animator.ClipBindingConstant ?? ClipConverter.ConvertValueArrayToGenericBinding(m_Clip);
                for (int frameIndex = 1; frameIndex < streamedFrames.Length - 1; frameIndex++)
                {
                    var frame = streamedFrames[frameIndex];
                    var streamedValues = frame.KeyList.Select(x => x.Value).ToArray();
                    for (int curveIndex = 0; curveIndex < frame.KeyList.Length;)
                    {
                        ReadCurveData(cachedData, m_ClipBindingConstant, frame.KeyList[curveIndex].Index, 
                            frame.Time, streamedValues, 0, ref curveIndex);
                    }
                }
                var m_DenseClip = m_Clip.DenseClip;
                var streamCount = m_Clip.StreamedClip.CurveCount;
                for (int frameIndex = 0; frameIndex < m_DenseClip.FrameCount; frameIndex++)
                {
                    var time = m_DenseClip.BeginTime + frameIndex / m_DenseClip.SampleRate;
                    var frameOffset = frameIndex * m_DenseClip.CurveCount;
                    for (int curveIndex = 0; curveIndex < m_DenseClip.CurveCount;)
                    {
                        var index = streamCount + curveIndex;
                        ReadCurveData(cachedData, m_ClipBindingConstant, (int)index, time, m_DenseClip.SampleArray, (int)frameOffset, ref curveIndex);
                    }
                }
                if (m_Clip.ConstantClip != null)
                {
                    var m_ConstantClip = m_Clip.ConstantClip.Value;
                    var denseCount = m_Clip.DenseClip.CurveCount;
                    var time2 = 0.0f;
                    for (int i = 0; i < 2; i++)
                    {
                        for (int curveIndex = 0; curveIndex < m_ConstantClip.Data.Length;)
                        {
                            var index = streamCount + denseCount + curveIndex;
                            ReadCurveData(cachedData, m_ClipBindingConstant, (int)index, time2, m_ConstantClip.Data, 0, ref curveIndex);
                        }
                        time2 = animator.MuscleClip.StopTime;
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
                                Input = _root.CreateAccessor($"Times_{track.Key}_{i}", track.Value[i].ToArray(), true),
                                Output = _root.CreateVectorAccessor("AnyCurve", track.Value[i + 1].ToArray(), track.Value[i].Count),
                                Interpolation = AnimationInterpolationMode.LINEAR
                            }),
                            Target = new()
                            {
                                Node = track.Key, // TODO find
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

        private void ReadCurveData(Dictionary<int, List<float>[]> cachedData, 
            AnimationClipBindingConstant m_ClipBindingConstant, 
            int index, float time, 
            float[] data, 
            int offset, ref int curveIndex)
        {
            var binding = AnimationClipBindingConstantConverter.FindBinding(m_ClipBindingConstant, index);
            if (binding.TypeID == NativeClassID.SkinnedMeshRenderer) //BlendShape
            {
                var channelName = GetChannelNameFromHash(binding.Attribute);
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

                var bPath = GetPathFromHash(binding.Path);
                var nodeIndex = string.IsNullOrEmpty(bPath) ?
                    GetPathByChannelName(channelName) : FindNode(bPath);
                if (!cachedData.TryGetValue(nodeIndex, out var track))
                {
                    track = CreateArray(10);
                    cachedData.Add(nodeIndex, track);
                }
                // channelName;
                // BlendShape
                track[0].Add(time);
                track[1].Add(data[curveIndex++ + offset]);
            }
            else if (binding.TypeID == NativeClassID.Transform)
            {
                var nodeIndex = FindNode(GetPathFromHash(binding.Path));
                if (!cachedData.TryGetValue(nodeIndex, out var track))
                {
                    track = CreateArray(10);
                    cachedData.Add(nodeIndex, track);
                }

                switch (binding.Attribute)
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
            _bonePathHash[Crc32.HashToUInt32(bytes)] = name;
            int index;
            while ((index = name.IndexOf('/')) >= 0)
            {
                name = name[(index + 1)..];
                bytes = Encoding.UTF8.GetBytes(name);
                _bonePathHash[Crc32.HashToUInt32(bytes)] = name;
            }
            foreach (var pptr in m_Transform.Children)
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
                boneName = AvatarConverter.FindBonePath(_avatar, hash);
            }
            if (string.IsNullOrEmpty(boneName))
            {
                boneName = "unknown " + hash;
            }
            return boneName;
        }

        private int GetPathByChannelName(string channelName)
        {
            if (_channelPathItems.TryGetValue(channelName, out var item)) 
            {
                return item;
            }
            return -1;
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
