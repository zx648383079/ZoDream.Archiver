﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Numerics;
using System.Text;
using ZoDream.AutodeskExporter;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class FbxExporter(IBundleContainer container) : IFbxImported, IMultipartExporter
    {

        public FbxImportedFrame RootFrame { get; protected set; }
        public List<FbxImportedMesh> MeshList { get; protected set; } = [];
        public List<FbxImportedMaterial> MaterialList { get; protected set; } = [];
        public List<FbxImportedTexture> TextureList { get; protected set; } = [];
        public List<FbxImportedKeyframedAnimation> AnimationList { get; protected set; } = [];
        public List<FbxImportedMorph> MorphList { get; protected set; } = [];

        private Avatar _avatar;
        private readonly HashSet<AnimationClip> _animationClipHashSet = [];
        private readonly Dictionary<AnimationClip, string> _boundAnimationPathDic = [];
        private readonly Dictionary<uint, string> _bonePathHash = [];
        private readonly Dictionary<Texture2D, string> _textureNameDictionary = [];
        private readonly Dictionary<Transform, FbxImportedFrame> _transformDictionary = [];
        private readonly Dictionary<uint, string> _morphChannelNames = [];
        public bool IsEmpty => MeshList.Count == 0 || RootFrame is null;
        public string Name { get; private set; } = string.Empty;
        public void Append(GameObject obj)
        {
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(obj.Name))
            {
                Name = obj.Name;
            }
            if (obj.m_Animator != null)
            {
                InitWithAnimator(obj.m_Animator);
                CollectAnimationClip(obj.m_Animator);
            }
            else
            {
                InitWithGameObject(obj);
            }
        }

        public void Append(Mesh mesh)
        {
            //if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(mesh.Name))
            //{
            //    Name = mesh.Name;
            //}
        }

        public void Append(Animator animator)
        {
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(animator.Name))
            {
                Name = animator.Name;
            }
            InitWithAnimator(animator);
            CollectAnimationClip(animator);
        }

        public void Append(AnimationClip animator)
        {
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(animator.Name))
            {
                Name = animator.Name;
            }
            _animationClipHashSet.Add(animator);
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (IsEmpty || !LocationStorage.TryCreate(fileName, ".fbx", mode, out fileName))
            {
                return;
            }
            ConvertAnimations();
            var folder = Path.GetDirectoryName(fileName);
            var currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(folder);

            var name = Path.GetFileName(fileName);

            using (var exporter = new FbxContext(name, this, true, true, false, 
                10, false, 1))
            {
                exporter.Initialize();
                exporter.ExportAll(true, true, true, .25f);
            }

            Directory.SetCurrentDirectory(currentDir);
        }

        private void InitWithAnimator(Animator m_Animator)
        {
            if (m_Animator.m_Avatar.TryGet(out var m_Avatar))
            {
                _avatar = m_Avatar;
            }
            container.TryAddExclude(m_Animator.m_GameObject.m_PathID);
            m_Animator.m_GameObject.TryGet(out var m_GameObject);
            InitWithGameObject(m_GameObject, m_Animator.m_HasTransformHierarchy);
        }

        private void InitWithGameObject(GameObject m_GameObject, bool hasTransformHierarchy = true)
        {
            var m_Transform = m_GameObject.m_Transform;
            if (!hasTransformHierarchy)
            {
                ConvertTransforms(m_Transform, null);
                DeoptimizeTransformHierarchy();
            }
            else
            {
                var frameList = new List<FbxImportedFrame>();
                var tempTransform = m_Transform;
                while (tempTransform.m_Father.TryGet(out var m_Father))
                {
                    frameList.Add(ConvertTransform(m_Father));
                    tempTransform = m_Father;
                }
                if (frameList.Count > 0)
                {
                    // 只能导出单个模型
                    RootFrame = frameList[^1];
                    for (var i = frameList.Count - 2; i >= 0; i--)
                    {
                        var frame = frameList[i];
                        var parent = frameList[i + 1];
                        parent.AddChild(frame);
                    }
                    ConvertTransforms(m_Transform, frameList[0]);
                }
                else
                {
                    ConvertTransforms(m_Transform, null);
                }

                CreateBonePathHash(m_Transform);
            }

            ConvertMeshRenderer(m_Transform);
        }

        private void ConvertMeshRenderer(Transform m_Transform)
        {
            m_Transform.m_GameObject.TryGet(out var m_GameObject);

            if (m_GameObject.m_MeshRenderer != null)
            {
                ConvertMeshRenderer(m_GameObject.m_MeshRenderer);
            }

            if (m_GameObject.m_SkinnedMeshRenderer != null)
            {
                ConvertMeshRenderer(m_GameObject.m_SkinnedMeshRenderer);
            }

            if (m_GameObject.m_Animation != null)
            {
                foreach (var animation in m_GameObject.m_Animation.m_Animations)
                {
                    if (animation.TryGet(out var animationClip))
                    {
                        if (!_boundAnimationPathDic.ContainsKey(animationClip))
                        {
                            _boundAnimationPathDic.Add(animationClip, GetTransformPath(m_Transform));
                        }
                        _animationClipHashSet.Add(animationClip);
                    }
                }
            }

            foreach (var pptr in m_Transform.m_Children)
            {
                if (pptr.TryGet(out var child))
                {
                    ConvertMeshRenderer(child);
                }
            }
        }

        private void CollectAnimationClip(Animator m_Animator)
        {
            if (m_Animator.m_Controller.TryGet(out var m_Controller))
            {
                switch (m_Controller)
                {
                    case AnimatorOverrideController m_AnimatorOverrideController:
                        {
                            if (m_AnimatorOverrideController.m_Controller.TryGet<AnimatorController>(out var m_AnimatorController))
                            {
                                foreach (var pptr in m_AnimatorController.m_AnimationClips)
                                {
                                    if (pptr.TryGet(out var m_AnimationClip))
                                    {
                                        _animationClipHashSet.Add(m_AnimationClip);
                                    }
                                }
                            }
                            break;
                        }

                    case AnimatorController m_AnimatorController:
                        {
                            foreach (var pptr in m_AnimatorController.m_AnimationClips)
                            {
                                if (pptr.TryGet(out var m_AnimationClip))
                                {
                                    _animationClipHashSet.Add(m_AnimationClip);
                                }
                            }
                            break;
                        }
                }
            }
        }

        private FbxImportedFrame ConvertTransform(Transform trans)
        {
            if (_transformDictionary.TryGetValue(trans, out var frame))
            {
                return frame;
            }
            frame = new FbxImportedFrame(trans.m_Children.Count);
            _transformDictionary.Add(trans, frame);
            trans.m_GameObject.TryGet(out var m_GameObject);
            frame.Name = m_GameObject.m_Name;
            SetFrame(frame, trans.m_LocalPosition, trans.m_LocalRotation, trans.m_LocalScale);
            return frame;
        }

        private static FbxImportedFrame CreateFrame(string name, Vector3 t, Quaternion q, Vector3 s)
        {
            var frame = new FbxImportedFrame
            {
                Name = name
            };
            SetFrame(frame, t, q, s);
            return frame;
        }

        private static void SetFrame(FbxImportedFrame frame, Vector3 t, Quaternion q, Vector3 s)
        {
            frame.LocalPosition = new Vector3(-t.X, t.Y, t.Z);
            frame.LocalRotation = FbxContext.ToEuler(new Quaternion(q.X, -q.Y, -q.Z, q.W));
            frame.LocalScale = s;
        }

        private void ConvertTransforms(Transform trans, FbxImportedFrame parent)
        {
            var frame = ConvertTransform(trans);
            if (parent == null)
            {
                // 只能导出单个模型
                RootFrame = frame;
            }
            else
            {
                parent.AddChild(frame);
            }
            foreach (var pptr in trans.m_Children)
            {
                if (pptr.TryGet(out var child))
                {
                    ConvertTransforms(child, frame);
                }
            }
        }

        private void ConvertMeshRenderer(UIRenderer meshR)
        {
            var mesh = GetMesh(meshR);
            if (mesh == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(mesh.Name))
            {
                Name = mesh.Name;
            }
            var iMesh = new FbxImportedMesh();
            meshR.m_GameObject.TryGet(out var m_GameObject2);
            iMesh.Path = GetTransformPath(m_GameObject2.m_Transform);
            iMesh.SubmeshList = new List<FbxImportedSubmesh>();
            var subHashSet = new HashSet<int>();
            var combine = false;
            int firstSubMesh = 0;
            if (meshR.m_StaticBatchInfo?.subMeshCount > 0)
            {
                firstSubMesh = meshR.m_StaticBatchInfo.firstSubMesh;
                var finalSubMesh = meshR.m_StaticBatchInfo.firstSubMesh + meshR.m_StaticBatchInfo.subMeshCount;
                for (int i = meshR.m_StaticBatchInfo.firstSubMesh; i < finalSubMesh; i++)
                {
                    subHashSet.Add(i);
                }
                combine = true;
            }
            else if (meshR.m_SubsetIndices?.Length > 0)
            {
                firstSubMesh = (int)meshR.m_SubsetIndices.Min(x => x);
                foreach (var index in meshR.m_SubsetIndices)
                {
                    subHashSet.Add((int)index);
                }
                combine = true;
            }

            iMesh.hasNormal = mesh.m_Normals?.Length > 0;
            iMesh.hasUV = new bool[8];
            for (int uv = 0; uv < 8; uv++)
            {
                iMesh.hasUV[uv] = mesh.GetUV(uv)?.Length > 0;
            }
            iMesh.hasTangent = mesh.m_Tangents != null && mesh.m_Tangents.Length == mesh.m_VertexCount * 4;
            iMesh.hasColor = mesh.m_Colors?.Length > 0;

            int firstFace = 0;
            for (int i = 0; i < mesh.m_SubMeshes.Count; i++)
            {
                int numFaces = (int)mesh.m_SubMeshes[i].indexCount / 3;
                if (subHashSet.Count > 0 && !subHashSet.Contains(i))
                {
                    firstFace += numFaces;
                    continue;
                }
                var submesh = mesh.m_SubMeshes[i];
                var iSubmesh = new FbxImportedSubmesh();
                Material mat = null;
                if (i - firstSubMesh < meshR.m_Materials.Count)
                {
                    if (meshR.m_Materials[i - firstSubMesh].TryGet(out var m_Material))
                    {
                        mat = m_Material;
                    }
                }
                FbxImportedMaterial iMat = ConvertMaterial(mat);
                iSubmesh.Material = iMat.Name;
                iSubmesh.BaseVertex = (int)mesh.m_SubMeshes[i].firstVertex;

                //Face
                iSubmesh.FaceList = new List<FbxImportedFace>(numFaces);
                var end = firstFace + numFaces;
                for (int f = firstFace; f < end; f++)
                {
                    var face = new FbxImportedFace();
                    face.VertexIndices = new int[3];
                    face.VertexIndices[0] = (int)(mesh.m_Indices[f * 3 + 2] - submesh.firstVertex);
                    face.VertexIndices[1] = (int)(mesh.m_Indices[f * 3 + 1] - submesh.firstVertex);
                    face.VertexIndices[2] = (int)(mesh.m_Indices[f * 3] - submesh.firstVertex);
                    iSubmesh.FaceList.Add(face);
                }
                firstFace = end;

                iMesh.SubmeshList.Add(iSubmesh);
            }

            // Shared vertex list
            iMesh.VertexList = new List<FbxImportedVertex>((int)mesh.m_VertexCount);
            for (var j = 0; j < mesh.m_VertexCount; j++)
            {
                var iVertex = new FbxImportedVertex();
                //Vertices
                int c = 3;
                if (mesh.m_Vertices.Length == mesh.m_VertexCount * 4)
                {
                    c = 4;
                }
                iVertex.Vertex = new Vector3(-mesh.m_Vertices[j * c], mesh.m_Vertices[j * c + 1], mesh.m_Vertices[j * c + 2]);
                //Normals
                if (iMesh.hasNormal)
                {
                    if (mesh.m_Normals.Length == mesh.m_VertexCount * 3)
                    {
                        c = 3;
                    }
                    else if (mesh.m_Normals.Length == mesh.m_VertexCount * 4)
                    {
                        c = 4;
                    }
                    iVertex.Normal = new Vector3(-mesh.m_Normals[j * c], mesh.m_Normals[j * c + 1], mesh.m_Normals[j * c + 2]);
                }
                //UV
                iVertex.UV = new float[8][];
                for (int uv = 0; uv < 8; uv++)
                {
                    if (iMesh.hasUV[uv])
                    {
                        var m_UV = mesh.GetUV(uv);
                        if (m_UV.Length == mesh.m_VertexCount * 2)
                        {
                            c = 2;
                        }
                        else if (m_UV.Length == mesh.m_VertexCount * 3)
                        {
                            c = 3;
                        }
                        iVertex.UV[uv] = [m_UV[j * c], m_UV[j * c + 1]];
                    }
                }
                //Tangent
                if (iMesh.hasTangent)
                {
                    iVertex.Tangent = new Vector4(-mesh.m_Tangents[j * 4], mesh.m_Tangents[j * 4 + 1], mesh.m_Tangents[j * 4 + 2], mesh.m_Tangents[j * 4 + 3]);
                }
                //Colors
                if (iMesh.hasColor)
                {
                    if (mesh.m_Colors.Length == mesh.m_VertexCount * 3)
                    {
                        iVertex.Color = new(mesh.m_Colors[j * 3], mesh.m_Colors[j * 3 + 1], mesh.m_Colors[j * 3 + 2], 1.0f);
                    }
                    else
                    {
                        iVertex.Color = new(mesh.m_Colors[j * 4], mesh.m_Colors[j * 4 + 1], mesh.m_Colors[j * 4 + 2], mesh.m_Colors[j * 4 + 3]);
                    }
                }
                //BoneInfluence
                if (mesh.m_Skin?.Count > 0)
                {
                    var inf = mesh.m_Skin[j];
                    iVertex.BoneIndices = new int[4];
                    iVertex.Weights = new float[4];
                    for (var k = 0; k < 4; k++)
                    {
                        iVertex.BoneIndices[k] = inf.boneIndex[k];
                        iVertex.Weights[k] = inf.weight[k];
                    }
                }
                iMesh.VertexList.Add(iVertex);
            }

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
                                var verifiedBoneCount2 = mesh.m_BoneNameHashes.Count(x => FixBonePath(GetPathFromHash(x)) != null);
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
                        var verifiedBoneCount = mesh.m_BoneNameHashes.Count(x => FixBonePath(GetPathFromHash(x)) != null);
                        if (verifiedBoneCount > 0)
                        {
                            boneType = 2;
                        }
                    }
                }

                if (boneType == 1)
                {
                    var boneCount = sMesh.m_Bones.Count;
                    iMesh.BoneList = new List<FbxImportedBone>(boneCount);
                    for (int i = 0; i < boneCount; i++)
                    {
                        var bone = new FbxImportedBone();
                        if (sMesh.m_Bones[i].TryGet(out var m_Transform))
                        {
                            bone.Path = GetTransformPath(m_Transform);
                        }
                        var convert = Matrix4x4.CreateScale(new Vector3(-1, 1, 1));
                        bone.Matrix = convert * mesh.m_BindPose[i] * convert;
                        iMesh.BoneList.Add(bone);
                    }
                }
                else if (boneType == 2)
                {
                    var boneCount = mesh.m_BindPose.Length;
                    iMesh.BoneList = new List<FbxImportedBone>(boneCount);
                    for (int i = 0; i < boneCount; i++)
                    {
                        var bone = new FbxImportedBone();
                        var boneHash = mesh.m_BoneNameHashes[i];
                        var path = GetPathFromHash(boneHash);
                        bone.Path = FixBonePath(path);
                        var convert = Matrix4x4.CreateScale(new Vector3(-1, 1, 1));
                        bone.Matrix = convert * mesh.m_BindPose[i] * convert;
                        iMesh.BoneList.Add(bone);
                    }
                }

                //Morphs
                if (mesh.m_Shapes?.channels?.Count > 0)
                {
                    var morph = new FbxImportedMorph();
                    MorphList.Add(morph);
                    morph.Path = iMesh.Path;
                    morph.Channels = new List<FbxImportedMorphChannel>(mesh.m_Shapes.channels.Count);
                    for (int i = 0; i < mesh.m_Shapes.channels.Count; i++)
                    {
                        var channel = new FbxImportedMorphChannel();
                        morph.Channels.Add(channel);
                        var shapeChannel = mesh.m_Shapes.channels[i];

                        var blendShapeName = "blendShape." + shapeChannel.name;
                        var bytes = Encoding.UTF8.GetBytes(blendShapeName);
                        _morphChannelNames[Crc32.HashToUInt32(bytes)] = blendShapeName;

                        channel.Name = shapeChannel.name.Split('.').Last();
                        channel.KeyframeList = new List<FbxImportedMorphKeyframe>(shapeChannel.frameCount);
                        var frameEnd = shapeChannel.frameIndex + shapeChannel.frameCount;
                        for (int frameIdx = shapeChannel.frameIndex; frameIdx < frameEnd; frameIdx++)
                        {
                            var keyframe = new FbxImportedMorphKeyframe();
                            channel.KeyframeList.Add(keyframe);
                            keyframe.Weight = mesh.m_Shapes.fullWeights[frameIdx];
                            var shape = mesh.m_Shapes.shapes[frameIdx];
                            keyframe.hasNormals = shape.hasNormals;
                            keyframe.hasTangents = shape.hasTangents;
                            keyframe.VertexList = new List<FbxImportedMorphVertex>((int)shape.vertexCount);
                            var vertexEnd = shape.firstVertex + shape.vertexCount;
                            for (uint j = shape.firstVertex; j < vertexEnd; j++)
                            {
                                var destVertex = new FbxImportedMorphVertex();
                                keyframe.VertexList.Add(destVertex);
                                var morphVertex = mesh.m_Shapes.vertices[(int)j];
                                destVertex.Index = morphVertex.index;
                                var sourceVertex = iMesh.VertexList[(int)morphVertex.index];
                                destVertex.Vertex = new FbxImportedVertex();
                                var morphPos = morphVertex.vertex;
                                destVertex.Vertex.Vertex = sourceVertex.Vertex + new Vector3(-morphPos.X, morphPos.Y, morphPos.Z);
                                if (shape.hasNormals)
                                {
                                    var morphNormal = morphVertex.normal;
                                    destVertex.Vertex.Normal = new Vector3(-morphNormal.X, morphNormal.Y, morphNormal.Z);
                                }
                                if (shape.hasTangents)
                                {
                                    var morphTangent = morphVertex.tangent;
                                    destVertex.Vertex.Tangent = new Vector4(-morphTangent.X, morphTangent.Y, morphTangent.Z, 0);
                                }
                            }
                        }
                    }
                }
            }

            //TODO combine mesh
            if (combine)
            {
                meshR.m_GameObject.TryGet(out var m_GameObject);
                var frame = RootFrame.FindChild(m_GameObject.m_Name);
                if (frame != null)
                {
                    frame.LocalPosition = RootFrame.LocalPosition;
                    frame.LocalRotation = RootFrame.LocalRotation;
                    while (frame.Parent != null)
                    {
                        frame = frame.Parent;
                        frame.LocalPosition = RootFrame.LocalPosition;
                        frame.LocalRotation = RootFrame.LocalRotation;
                    }
                }
            }

            MeshList.Add(iMesh);
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
            if (_transformDictionary.TryGetValue(transform, out var frame))
            {
                return frame.Path;
            }
            return null;
        }

        private string FixBonePath(AnimationClip m_AnimationClip, string path)
        {
            if (_boundAnimationPathDic.TryGetValue(m_AnimationClip, out var basePath))
            {
                path = basePath + "/" + path;
            }
            return FixBonePath(path);
        }

        private string FixBonePath(string path)
        {
            var frame = RootFrame.FindFrameByPath(path);
            return frame?.Path;
        }

        internal static string GetTransformPathByFather(Transform transform)
        {
            transform.m_GameObject.TryGet(out var m_GameObject);
            if (transform.m_Father.TryGet(out var father))
            {
                return GetTransformPathByFather(father) + "/" + m_GameObject.m_Name;
            }

            return m_GameObject.m_Name;
        }

        private FbxImportedMaterial ConvertMaterial(Material mat)
        {
            FbxImportedMaterial iMat;
            if (mat != null)
            {
                iMat = FbxImportedHelpers.FindMaterial(mat.m_Name, MaterialList);
                if (iMat != null)
                {
                    return iMat;
                }
                iMat = new FbxImportedMaterial();
                iMat.Name = mat.m_Name;
                //default values
                iMat.Diffuse = new(0.8f, 0.8f, 0.8f, 1);
                iMat.Ambient = new(0.2f, 0.2f, 0.2f, 1);
                iMat.Emissive = new(0, 0, 0, 1);
                iMat.Specular = new(0.2f, 0.2f, 0.2f, 1);
                iMat.Reflection = new(0, 0, 0, 1);
                iMat.Shininess = 20f;
                iMat.Transparency = 0f;
                foreach (var col in mat.m_SavedProperties.m_Colors)
                {
                    switch (col.Key)
                    {
                        case "_Color":
                            iMat.Diffuse = col.Value;
                            break;
                        case "_SColor":
                            iMat.Ambient = col.Value;
                            break;
                        case "_EmissionColor":
                            iMat.Emissive = col.Value;
                            break;
                        case "_SpecularColor":
                            iMat.Specular = col.Value;
                            break;
                        case "_ReflectColor":
                            iMat.Reflection = col.Value;
                            break;
                    }
                }

                foreach (var flt in mat.m_SavedProperties.m_Floats)
                {
                    switch (flt.Key)
                    {
                        case "_Shininess":
                            iMat.Shininess = flt.Value;
                            break;
                        case "_Transparency":
                            iMat.Transparency = flt.Value;
                            break;
                    }
                }

                //textures
                iMat.Textures = [];
                foreach (var texEnv in mat.m_SavedProperties.m_TexEnvs)
                {
                    if (!texEnv.Value.m_Texture.TryGet<Texture2D>(out var m_Texture2D)) //TODO other Texture
                    {
                        continue;
                    }

                    var texture = new FbxImportedMaterialTexture();
                    iMat.Textures.Add(texture);

                    int dest = -1;
                    if (texEnv.Key == "_MainTex")
                        dest = 0;
                    else if (texEnv.Key == "_BumpMap")
                        dest = 3;
                    else if (texEnv.Key.Contains("Specular"))
                        dest = 2;
                    else if (texEnv.Key.Contains("Normal"))
                        dest = 1;

                    texture.Dest = dest;

                    var ext = $".png";
                    if (_textureNameDictionary.TryGetValue(m_Texture2D, out var textureName))
                    {
                        texture.Name = textureName;
                    }
                    //else if (FbxImportedHelpers.FindTexture(m_Texture2D.m_Name + ext, TextureList) != null) //已有相同名字的图片
                    //{
                    //    for (int i = 1; ; i++)
                    //    {
                    //        var name = m_Texture2D.m_Name + $" ({i}){ext}";
                    //        if (FbxImportedHelpers.FindTexture(name, TextureList) == null)
                    //        {
                    //            texture.Name = name;
                    //            _textureNameDictionary.Add(m_Texture2D, name);
                    //            break;
                    //        }
                    //    }
                    //}
                    else
                    {
                        texture.Name = m_Texture2D.m_Name + ext;
                        _textureNameDictionary.Add(m_Texture2D, texture.Name);
                    }

                    texture.Offset = texEnv.Value.m_Offset;
                    texture.Scale = texEnv.Value.m_Scale;
                    ConvertTexture2D(m_Texture2D, texture.Name);
                }

                MaterialList.Add(iMat);
            }
            else
            {
                iMat = new FbxImportedMaterial();
            }
            return iMat;
        }

        private void ConvertTexture2D(Texture2D m_Texture2D, string name)
        {
            var iTex = FbxImportedHelpers.FindTexture(name, TextureList);
            if (iTex != null)
            {
                return;
            }
            //using var image = m_Texture2D.ToImage();
            //if (image is null)
            //{
            //    return;
            //}
            //using var stream = new MemoryStream();
            //image.Encode(stream, SkiaSharp.SKEncodedImageFormat.Png, 100);
            iTex = new FbxImportedTexture(name);
            TextureList.Add(iTex);
        }

        private void ConvertAnimations()
        {
            foreach (var animationClip in _animationClipHashSet)
            {
                var iAnim = new FbxImportedKeyframedAnimation();
                var name = animationClip.m_Name;
                if (AnimationList.Exists(x => x.Name == name))
                {
                    for (int i = 1; ; i++)
                    {
                        var fixName = name + $"_{i}";
                        if (!AnimationList.Exists(x => x.Name == fixName))
                        {
                            name = fixName;
                            break;
                        }
                    }
                }
                iAnim.Name = name;
                iAnim.SampleRate = animationClip.m_SampleRate;
                iAnim.TrackList = [];
                AnimationList.Add(iAnim);
                if (animationClip.m_Legacy)
                {
                    foreach (var m_CompressedRotationCurve in animationClip.m_CompressedRotationCurves)
                    {
                        var track = iAnim.FindTrack(FixBonePath(animationClip, m_CompressedRotationCurve.m_Path));

                        var numKeys = m_CompressedRotationCurve.m_Times.m_NumItems;
                        var data = m_CompressedRotationCurve.m_Times.UnpackInts();
                        var times = new float[numKeys];
                        int t = 0;
                        for (int i = 0; i < numKeys; i++)
                        {
                            t += data[i];
                            times[i] = t * 0.01f;
                        }
                        var quats = m_CompressedRotationCurve.m_Values.UnpackQuats();

                        for (int i = 0; i < numKeys; i++)
                        {
                            var quat = quats[i];
                            var value = FbxContext.ToEuler(new Quaternion(quat.X, -quat.Y, -quat.Z, quat.W));
                            track.Rotations.Add(new FbxImportedKeyframe<Vector3>(times[i], value));
                        }
                    }
                    foreach (var m_RotationCurve in animationClip.m_RotationCurves)
                    {
                        var track = iAnim.FindTrack(FixBonePath(animationClip, m_RotationCurve.path));
                        foreach (var m_Curve in m_RotationCurve.curve.m_Curve)
                        {
                            var value = FbxContext.ToEuler(new Quaternion(m_Curve.value.X, -m_Curve.value.Y, -m_Curve.value.Z, m_Curve.value.W));
                            track.Rotations.Add(new FbxImportedKeyframe<Vector3>(m_Curve.time, value));
                        }
                    }
                    foreach (var m_PositionCurve in animationClip.m_PositionCurves)
                    {
                        var track = iAnim.FindTrack(FixBonePath(animationClip, m_PositionCurve.path));
                        foreach (var m_Curve in m_PositionCurve.curve.m_Curve)
                        {
                            track.Translations.Add(new FbxImportedKeyframe<Vector3>(m_Curve.time, new Vector3(-m_Curve.value.X, m_Curve.value.Y, m_Curve.value.Z)));
                        }
                    }
                    foreach (var m_ScaleCurve in animationClip.m_ScaleCurves)
                    {
                        var track = iAnim.FindTrack(FixBonePath(animationClip, m_ScaleCurve.path));
                        foreach (var m_Curve in m_ScaleCurve.curve.m_Curve)
                        {
                            track.Scalings.Add(new FbxImportedKeyframe<Vector3>(m_Curve.time, new Vector3(m_Curve.value.X, m_Curve.value.Y, m_Curve.value.Z)));
                        }
                    }
                    if (animationClip.m_EulerCurves != null)
                    {
                        foreach (var m_EulerCurve in animationClip.m_EulerCurves)
                        {
                            var track = iAnim.FindTrack(FixBonePath(animationClip, m_EulerCurve.path));
                            foreach (var m_Curve in m_EulerCurve.curve.m_Curve)
                            {
                                track.Rotations.Add(new FbxImportedKeyframe<Vector3>(m_Curve.time, new Vector3(m_Curve.value.X, -m_Curve.value.Y, -m_Curve.value.Z)));
                            }
                        }
                    }
                    foreach (var m_FloatCurve in animationClip.m_FloatCurves)
                    {
                        if (m_FloatCurve.classID == ElementIDType.SkinnedMeshRenderer) //BlendShape
                        {
                            var channelName = m_FloatCurve.attribute;
                            int dotPos = channelName.IndexOf('.');
                            if (dotPos >= 0)
                            {
                                channelName = channelName.Substring(dotPos + 1);
                            }

                            var path = FixBonePath(animationClip, m_FloatCurve.path);
                            if (string.IsNullOrEmpty(path))
                            {
                                path = GetPathByChannelName(channelName);
                            }
                            var track = iAnim.FindTrack(path);
                            track.BlendShape = new FbxImportedBlendShape();
                            track.BlendShape.ChannelName = channelName;
                            foreach (var m_Curve in m_FloatCurve.curve.m_Curve)
                            {
                                track.BlendShape.Keyframes.Add(new FbxImportedKeyframe<float>(m_Curve.time, m_Curve.value));
                            }
                        }
                    }
                }
                else
                {
                    var m_Clip = animationClip.m_MuscleClip.m_Clip;
                    var streamedFrames = m_Clip.m_StreamedClip.ReadData();
                    var m_ClipBindingConstant = animationClip.m_ClipBindingConstant ?? m_Clip.ConvertValueArrayToGenericBinding();
                    for (int frameIndex = 1; frameIndex < streamedFrames.Count - 1; frameIndex++)
                    {
                        var frame = streamedFrames[frameIndex];
                        var streamedValues = frame.keyList.Select(x => x.value).ToArray();
                        for (int curveIndex = 0; curveIndex < frame.keyList.Count;)
                        {
                            ReadCurveData(iAnim, m_ClipBindingConstant, frame.keyList[curveIndex].index, frame.time, streamedValues, 0, ref curveIndex);
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
                            ReadCurveData(iAnim, m_ClipBindingConstant, (int)index, time, m_DenseClip.m_SampleArray, (int)frameOffset, ref curveIndex);
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
                                ReadCurveData(iAnim, m_ClipBindingConstant, (int)index, time2, m_ConstantClip.data, 0, ref curveIndex);
                            }
                            time2 = animationClip.m_MuscleClip.m_StopTime;
                        }
                    }
                }
            }
        }

        private void ReadCurveData(FbxImportedKeyframedAnimation iAnim, AnimationClipBindingConstant m_ClipBindingConstant, int index, float time, float[] data, int offset, ref int curveIndex)
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

                var bPath = FixBonePath(GetPathFromHash(binding.path));
                if (string.IsNullOrEmpty(bPath))
                {
                    bPath = GetPathByChannelName(channelName);
                }
                var bTrack = iAnim.FindTrack(bPath);
                bTrack.BlendShape = new FbxImportedBlendShape();
                bTrack.BlendShape.ChannelName = channelName;
                bTrack.BlendShape.Keyframes.Add(new FbxImportedKeyframe<float>(time, data[curveIndex++ + offset]));
            }
            else if (binding.typeID == ElementIDType.Transform)
            {
                var path = FixBonePath(GetPathFromHash(binding.path));
                var track = iAnim.FindTrack(path);

                switch (binding.attribute)
                {
                    case 1:
                        track.Translations.Add(new FbxImportedKeyframe<Vector3>(time, new Vector3
                        (
                            -data[curveIndex++ + offset],
                            data[curveIndex++ + offset],
                            data[curveIndex++ + offset]
                        )));
                        break;
                    case 2:
                        var value = FbxContext.ToEuler(new Quaternion
                        (
                            data[curveIndex++ + offset],
                            -data[curveIndex++ + offset],
                            -data[curveIndex++ + offset],
                            data[curveIndex++ + offset]
                        ));
                        track.Rotations.Add(new FbxImportedKeyframe<Vector3>(time, value));
                        break;
                    case 3:
                        track.Scalings.Add(new FbxImportedKeyframe<Vector3>(time, new Vector3
                        (
                            data[curveIndex++ + offset],
                            data[curveIndex++ + offset],
                            data[curveIndex++ + offset]
                        )));
                        break;
                    case 4:
                        track.Rotations.Add(new FbxImportedKeyframe<Vector3>(time, new Vector3
                        (
                            data[curveIndex++ + offset],
                            -data[curveIndex++ + offset],
                            -data[curveIndex++ + offset]
                        )));
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

        private void CreateBonePathHash(Transform m_Transform)
        {
            var name = GetTransformPathByFather(m_Transform);
            var bytes = Encoding.UTF8.GetBytes(name);
            _bonePathHash[Crc32.HashToUInt32(bytes)] = name;
            int index;
            while ((index = name.IndexOf('/')) >= 0)
            {
                name = name[(index + 1)..];
                bytes = Encoding.UTF8.GetBytes(name);
                _bonePathHash[Crc32.HashToUInt32(bytes)] = name;
            }
            foreach (var pptr in m_Transform.m_Children)
            {
                if (pptr.TryGet(out var child))
                {
                    CreateBonePathHash(child);
                }
            }
        }


        private void DeoptimizeTransformHierarchy()
        {
            if (_avatar == null)
                throw new Exception("Transform hierarchy has been optimized, but can't find Avatar to deoptimize.");
            // 1. Figure out the skeletonPaths from the unstripped avatar
            var skeletonPaths = new List<string>();
            foreach (var id in _avatar.m_Avatar.m_AvatarSkeleton.m_ID)
            {
                var path = _avatar.FindBonePath(id);
                skeletonPaths.Add(path);
            }
            // 2. Restore the original transform hierarchy
            // Prerequisite: skeletonPaths follow pre-order traversal
            for (var i = 1; i < skeletonPaths.Count; i++) // start from 1, skip the root transform because it will always be there.
            {
                var path = skeletonPaths[i];
                var strs = path.Split('/');
                string transformName;
                FbxImportedFrame parentFrame;
                if (strs.Length == 1)
                {
                    transformName = path;
                    parentFrame = RootFrame;
                }
                else
                {
                    transformName = strs.Last();
                    var parentFramePath = path.Substring(0, path.LastIndexOf('/'));
                    parentFrame = RootFrame.FindRelativeFrameWithPath(parentFramePath);
                }
                var skeletonPose = _avatar.m_Avatar.m_DefaultPose;
                var xform = skeletonPose.m_X[i];
                var frame = RootFrame.FindChild(transformName);
                if (frame != null)
                {
                    SetFrame(frame, xform.T, xform.Q, xform.S);
                }
                else
                {
                    frame = CreateFrame(transformName, xform.T, xform.Q, xform.S);
                }
                parentFrame.AddChild(frame);
            }
        }

        private string? GetPathByChannelName(string channelName)
        {
            foreach (var morph in MorphList)
            {
                foreach (var channel in morph.Channels)
                {
                    if (channel.Name == channelName)
                    {
                        return morph.Path;
                    }
                }
            }
            return null;
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

        public void Dispose()
        {
        }

    }
}
