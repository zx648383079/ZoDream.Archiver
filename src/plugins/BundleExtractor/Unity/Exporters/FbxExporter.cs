using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;
using ZoDream.AutodeskExporter;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class FbxExporter(ISerializedFile resource) : IFbxImported, IMultipartExporter
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
        public string FileName { get; private set; } = string.Empty;
        public string SourcePath => resource.FullPath;

        public void Append(int entryId)
        {
            var obj = resource[entryId];
            var fileId = resource.Get(entryId).FileID;
            if (string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(obj.Name))
            {
                FileName = obj.Name;
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
                InitWithAnimator(animator);
                CollectAnimationClip(animator);
            }
            else
            {
                InitWithGameObject(obj);
            }
        }

        public void Append(Mesh mesh)
        {
        }

        public void Append(Animator animator)
        {
            InitWithAnimator(animator);
            CollectAnimationClip(animator);
        }

        public void Append(AnimationClip animator)
        {
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

        private void InitWithAnimator(Animator animator)
        {
            if (animator.Avatar.TryGet(out var m_Avatar))
            {
                _avatar = m_Avatar;
            }
            resource.AddExclude(animator.GameObject.PathID);
            if (animator.GameObject.TryGet(out var game))
            {
                InitWithGameObject(game, animator.HasTransformHierarchy);
            }
        }

        private void InitWithGameObject(GameObject game, bool hasTransformHierarchy = true)
        {
            if (!GameObjectConverter.TryGet<Transform>(game, out var transform))
            {
                return;
            }
            if (!hasTransformHierarchy)
            {
                ConvertTransforms(transform, null);
                DeoptimizeTransformHierarchy();
            }
            else
            {
                var frameList = new List<FbxImportedFrame>();
                var tempTransform = transform;
                while (tempTransform.Father.TryGet(out var m_Father))
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
                    ConvertTransforms(transform, frameList[0]);
                }
                else
                {
                    ConvertTransforms(transform, null);
                }

                CreateBonePathHash(transform);
            }

            ConvertMeshRenderer(transform);
        }

        private void ConvertMeshRenderer(Transform transform)
        {
            if (transform.GameObject.TryGet(out var game))
            {
                foreach (var item in GameObjectConverter.ForEach<Component>(game))
                {
                    switch (item)
                    {
                        case Renderer renderer:
                            ConvertMeshRenderer(renderer);
                            break;
                        case Animation animation:
                            foreach (var pptr in animation.Clips)
                            {
                                if (pptr.TryGet(out var animationClip))
                                {
                                    if (!_boundAnimationPathDic.ContainsKey(animationClip))
                                    {
                                        _boundAnimationPathDic.Add(animationClip, GetTransformPath(transform));
                                    }
                                    _animationClipHashSet.Add(animationClip);
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
                if (pptr.TryGet(out var child))
                {
                    ConvertMeshRenderer(child);
                }
            }
        }

        private void CollectAnimationClip(Animator m_Animator)
        {
            if (m_Animator.Controller.TryGet(out var m_Controller))
            {
                switch (m_Controller)
                {
                    case AnimatorOverrideController m_AnimatorOverrideController:
                        {
                            if (m_AnimatorOverrideController.Controller.TryGet<AnimatorController>(out var m_AnimatorController))
                            {
                                foreach (var pptr in m_AnimatorController.AnimationClips)
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
                            foreach (var pptr in m_AnimatorController.AnimationClips)
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
            frame = new FbxImportedFrame(trans.Children.Length);
            _transformDictionary.Add(trans, frame);
            trans.GameObject.TryGet(out var m_GameObject);
            frame.Name = m_GameObject.Name;
            SetFrame(frame, trans.LocalPosition, trans.LocalRotation, trans.LocalScale);
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
            foreach (var pptr in trans.Children)
            {
                if (pptr.TryGet(out var child))
                {
                    ConvertTransforms(child, frame);
                }
            }
        }

        private void ConvertMeshRenderer(Renderer meshR)
        {
            var mesh = GameObjectConverter.GetMesh(meshR);
            if (mesh == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(mesh.Name))
            {
                FileName = mesh.Name;
            }
            var iMesh = new FbxImportedMesh();
            if (meshR.GameObject.TryGet(out var game) && 
                GameObjectConverter.TryGet<Transform>(game, out var transform))
            {
                iMesh.Path = GetTransformPath(transform);
            }
            
            iMesh.SubmeshList = new List<FbxImportedSubmesh>();
            var subHashSet = new HashSet<int>();
            var combine = false;
            int firstSubMesh = 0;
            if (meshR.StaticBatchInfo.SubMeshCount > 0)
            {
                firstSubMesh = meshR.StaticBatchInfo.FirstSubMesh;
                var finalSubMesh = meshR.StaticBatchInfo.FirstSubMesh + meshR.StaticBatchInfo.SubMeshCount;
                for (int i = meshR.StaticBatchInfo.FirstSubMesh; i < finalSubMesh; i++)
                {
                    subHashSet.Add(i);
                }
                combine = true;
            }
            else if (meshR.SubsetIndices?.Length > 0)
            {
                firstSubMesh = (int)meshR.SubsetIndices.Min(x => x);
                foreach (var index in meshR.SubsetIndices)
                {
                    subHashSet.Add((int)index);
                }
                combine = true;
            }

            iMesh.hasNormal = mesh.Normals?.Length > 0;
            iMesh.hasUV = new bool[8];
            for (int uv = 0; uv < 8; uv++)
            {
                iMesh.hasUV[uv] = MeshConverter.GetUV(mesh, uv)?.Length > 0;
            }
            iMesh.hasTangent = mesh.Tangents != null && mesh.Tangents.Length == mesh.VertexCount * 4;
            iMesh.hasColor = mesh.Colors?.Length > 0;

            int firstFace = 0;
            for (int i = 0; i < mesh.SubMeshes.Length; i++)
            {
                int numFaces = (int)mesh.SubMeshes[i].IndexCount / 3;
                if (subHashSet.Count > 0 && !subHashSet.Contains(i))
                {
                    firstFace += numFaces;
                    continue;
                }
                var submesh = mesh.SubMeshes[i];
                var iSubmesh = new FbxImportedSubmesh();
                Material mat = null;
                if (i - firstSubMesh < meshR.Materials.Length)
                {
                    if (meshR.Materials[i - firstSubMesh].TryGet(out var m_Material))
                    {
                        mat = m_Material;
                    }
                }
                FbxImportedMaterial iMat = ConvertMaterial(mat);
                iSubmesh.Material = iMat.Name;
                iSubmesh.BaseVertex = (int)mesh.SubMeshes[i].FirstVertex;

                //Face
                iSubmesh.FaceList = new List<FbxImportedFace>(numFaces);
                var end = firstFace + numFaces;
                for (int f = firstFace; f < end; f++)
                {
                    var face = new FbxImportedFace();
                    face.VertexIndices = new int[3];
                    face.VertexIndices[0] = (int)(mesh.Indices[f * 3 + 2] - submesh.FirstVertex);
                    face.VertexIndices[1] = (int)(mesh.Indices[f * 3 + 1] - submesh.FirstVertex);
                    face.VertexIndices[2] = (int)(mesh.Indices[f * 3] - submesh.FirstVertex);
                    iSubmesh.FaceList.Add(face);
                }
                firstFace = end;

                iMesh.SubmeshList.Add(iSubmesh);
            }

            // Shared vertex list
            iMesh.VertexList = new List<FbxImportedVertex>((int)mesh.VertexCount);
            for (var j = 0; j < mesh.VertexCount; j++)
            {
                var iVertex = new FbxImportedVertex();
                //Vertices
                int c = 3;
                if (mesh.Vertices.Length == mesh.VertexCount * 4)
                {
                    c = 4;
                }
                iVertex.Vertex = new Vector3(-mesh.Vertices[j * c], mesh.Vertices[j * c + 1], mesh.Vertices[j * c + 2]);
                //Normals
                if (iMesh.hasNormal)
                {
                    if (mesh.Normals.Length == mesh.VertexCount * 3)
                    {
                        c = 3;
                    }
                    else if (mesh.Normals.Length == mesh.VertexCount * 4)
                    {
                        c = 4;
                    }
                    iVertex.Normal = new Vector3(-mesh.Normals[j * c], mesh.Normals[j * c + 1], mesh.Normals[j * c + 2]);
                }
                //UV
                iVertex.UV = new float[8][];
                for (int uv = 0; uv < 8; uv++)
                {
                    if (iMesh.hasUV[uv])
                    {
                        var m_UV = MeshConverter.GetUV(mesh, uv);
                        if (m_UV.Length == mesh.VertexCount * 2)
                        {
                            c = 2;
                        }
                        else if (m_UV.Length == mesh.VertexCount * 3)
                        {
                            c = 3;
                        }
                        iVertex.UV[uv] = [m_UV[j * c], m_UV[j * c + 1]];
                    }
                }
                //Tangent
                if (iMesh.hasTangent)
                {
                    iVertex.Tangent = new Vector4(-mesh.Tangents[j * 4], mesh.Tangents[j * 4 + 1], mesh.Tangents[j * 4 + 2], mesh.Tangents[j * 4 + 3]);
                }
                //Colors
                if (iMesh.hasColor)
                {
                    if (mesh.Colors.Length == mesh.VertexCount * 3)
                    {
                        iVertex.Color = new(mesh.Colors[j * 3], mesh.Colors[j * 3 + 1], mesh.Colors[j * 3 + 2], 1.0f);
                    }
                    else
                    {
                        iVertex.Color = new(mesh.Colors[j * 4], mesh.Colors[j * 4 + 1], mesh.Colors[j * 4 + 2], mesh.Colors[j * 4 + 3]);
                    }
                }
                //BoneInfluence
                if (mesh.Skin?.Length > 0)
                {
                    var inf = mesh.Skin[j];
                    iVertex.BoneIndices = new int[4];
                    iVertex.Weights = new float[4];
                    for (var k = 0; k < 4; k++)
                    {
                        iVertex.BoneIndices[k] = inf.BoneIndex[k];
                        iVertex.Weights[k] = inf.Weight[k];
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
                                var verifiedBoneCount2 = mesh.BoneNameHashes.Count(x => FixBonePath(GetPathFromHash(x)) != null);
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
                        var verifiedBoneCount = mesh.BoneNameHashes.Count(x => FixBonePath(GetPathFromHash(x)) != null);
                        if (verifiedBoneCount > 0)
                        {
                            boneType = 2;
                        }
                    }
                }

                if (boneType == 1)
                {
                    var boneCount = sMesh.Bones.Length;
                    iMesh.BoneList = new List<FbxImportedBone>(boneCount);
                    for (int i = 0; i < boneCount; i++)
                    {
                        var bone = new FbxImportedBone();
                        if (sMesh.Bones[i].TryGet(out var m_Transform))
                        {
                            bone.Path = GetTransformPath(m_Transform);
                        }
                        var convert = Matrix4x4.CreateScale(new Vector3(-1, 1, 1));
                        bone.Matrix = convert * mesh.BindPose[i] * convert;
                        iMesh.BoneList.Add(bone);
                    }
                }
                else if (boneType == 2)
                {
                    var boneCount = mesh.BindPose.Length;
                    iMesh.BoneList = new List<FbxImportedBone>(boneCount);
                    for (int i = 0; i < boneCount; i++)
                    {
                        var bone = new FbxImportedBone();
                        var boneHash = mesh.BoneNameHashes[i];
                        var path = GetPathFromHash(boneHash);
                        bone.Path = FixBonePath(path);
                        var convert = Matrix4x4.CreateScale(new Vector3(-1, 1, 1));
                        bone.Matrix = convert * mesh.BindPose[i] * convert;
                        iMesh.BoneList.Add(bone);
                    }
                }

                //Morphs
                if (mesh.Shapes?.Channels?.Length > 0)
                {
                    var morph = new FbxImportedMorph();
                    MorphList.Add(morph);
                    morph.Path = iMesh.Path;
                    morph.Channels = new List<FbxImportedMorphChannel>(mesh.Shapes.Channels.Length);
                    for (int i = 0; i < mesh.Shapes.Channels.Length; i++)
                    {
                        var channel = new FbxImportedMorphChannel();
                        morph.Channels.Add(channel);
                        var shapeChannel = mesh.Shapes.Channels[i];

                        var blendShapeName = "blendShape." + shapeChannel.Name;
                        var bytes = Encoding.UTF8.GetBytes(blendShapeName);
                        _morphChannelNames[Crc32.HashToUInt32(bytes)] = blendShapeName;

                        channel.Name = shapeChannel.Name.Split('.').Last();
                        channel.KeyframeList = new List<FbxImportedMorphKeyframe>(shapeChannel.FrameCount);
                        var frameEnd = shapeChannel.FrameIndex + shapeChannel.FrameCount;
                        for (int frameIdx = shapeChannel.FrameIndex; frameIdx < frameEnd; frameIdx++)
                        {
                            var keyframe = new FbxImportedMorphKeyframe();
                            channel.KeyframeList.Add(keyframe);
                            keyframe.Weight = mesh.Shapes.FullWeights[frameIdx];
                            var shape = mesh.Shapes.Shapes[frameIdx];
                            keyframe.hasNormals = shape.HasNormals;
                            keyframe.hasTangents = shape.HasTangents;
                            keyframe.VertexList = new List<FbxImportedMorphVertex>((int)shape.VertexCount);
                            var vertexEnd = shape.FirstVertex + shape.VertexCount;
                            for (uint j = shape.FirstVertex; j < vertexEnd; j++)
                            {
                                var destVertex = new FbxImportedMorphVertex();
                                keyframe.VertexList.Add(destVertex);
                                var morphVertex = mesh.Shapes.Vertices[(int)j];
                                destVertex.Index = morphVertex.Index;
                                var sourceVertex = iMesh.VertexList[(int)morphVertex.Index];
                                destVertex.Vertex = new FbxImportedVertex();
                                var morphPos = morphVertex.Vertex;
                                destVertex.Vertex.Vertex = sourceVertex.Vertex + new Vector3(-morphPos.X, morphPos.Y, morphPos.Z);
                                if (shape.HasNormals)
                                {
                                    var morphNormal = morphVertex.Normal;
                                    destVertex.Vertex.Normal = new Vector3(-morphNormal.X, morphNormal.Y, morphNormal.Z);
                                }
                                if (shape.HasTangents)
                                {
                                    var morphTangent = morphVertex.Tangent;
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
                meshR.GameObject.TryGet(out var m_GameObject);
                var frame = RootFrame.FindChild(m_GameObject.Name);
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
            transform.GameObject.TryGet(out var m_GameObject);
            if (transform.Father.TryGet(out var father))
            {
                return GetTransformPathByFather(father) + "/" + m_GameObject.Name;
            }

            return m_GameObject.Name;
        }

        private FbxImportedMaterial ConvertMaterial(Material mat)
        {
            FbxImportedMaterial iMat;
            if (mat != null)
            {
                iMat = FbxImportedHelpers.FindMaterial(mat.Name, MaterialList);
                if (iMat != null)
                {
                    return iMat;
                }
                iMat = new FbxImportedMaterial();
                iMat.Name = mat.Name;
                //default values
                iMat.Diffuse = new(0.8f, 0.8f, 0.8f, 1);
                iMat.Ambient = new(0.2f, 0.2f, 0.2f, 1);
                iMat.Emissive = new(0, 0, 0, 1);
                iMat.Specular = new(0.2f, 0.2f, 0.2f, 1);
                iMat.Reflection = new(0, 0, 0, 1);
                iMat.Shininess = 20f;
                iMat.Transparency = 0f;
                foreach (var col in mat.SavedProperties.Colors)
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

                foreach (var flt in mat.SavedProperties.Floats)
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
                foreach (var texEnv in mat.SavedProperties.TexEnvs)
                {
                    if (!texEnv.Value.Texture.TryGet<Texture2D>(out var m_Texture2D)) //TODO other Texture
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
                    //else if (FbxImportedHelpers.FindTexture(m_Texture2D.Name + ext, TextureList) != null) //已有相同名字的图片
                    //{
                    //    for (int i = 1; ; i++)
                    //    {
                    //        var name = m_Texture2D.Name + $" ({i}){ext}";
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
                        texture.Name = m_Texture2D.Name + ext;
                        _textureNameDictionary.Add(m_Texture2D, texture.Name);
                    }

                    texture.Offset = texEnv.Value.Offset;
                    texture.Scale = texEnv.Value.Scale;
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
                var name = animationClip.Name;
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
                iAnim.SampleRate = animationClip.SampleRate;
                iAnim.TrackList = [];
                AnimationList.Add(iAnim);
                if (animationClip.Legacy)
                {
                    foreach (var m_CompressedRotationCurve in animationClip.CompressedRotationCurves)
                    {
                        var track = iAnim.FindTrack(FixBonePath(animationClip, m_CompressedRotationCurve.Path));

                        var numKeys = m_CompressedRotationCurve.Times.NumItems;
                        var data = PackedIntVectorConverter.UnpackInts(m_CompressedRotationCurve.Times);
                        var times = new float[numKeys];
                        int t = 0;
                        for (int i = 0; i < numKeys; i++)
                        {
                            t += data[i];
                            times[i] = t * 0.01f;
                        }
                        var quats = PackedQuatVectorConverter.UnpackQuats(m_CompressedRotationCurve.Values);

                        for (int i = 0; i < numKeys; i++)
                        {
                            var quat = quats[i];
                            var value = FbxContext.ToEuler(new Quaternion(quat.X, -quat.Y, -quat.Z, quat.W));
                            track.Rotations.Add(new FbxImportedKeyframe<Vector3>(times[i], value));
                        }
                    }
                    foreach (var m_RotationCurve in animationClip.RotationCurves)
                    {
                        var track = iAnim.FindTrack(FixBonePath(animationClip, m_RotationCurve.Path));
                        foreach (var m_Curve in m_RotationCurve.Curve.Curve)
                        {
                            var value = FbxContext.ToEuler(new Quaternion(m_Curve.Value.X, -m_Curve.Value.Y, -m_Curve.Value.Z, m_Curve.Value.W));
                            track.Rotations.Add(new FbxImportedKeyframe<Vector3>(m_Curve.Time, value));
                        }
                    }
                    foreach (var m_PositionCurve in animationClip.PositionCurves)
                    {
                        var track = iAnim.FindTrack(FixBonePath(animationClip, m_PositionCurve.Path));
                        foreach (var m_Curve in m_PositionCurve.Curve.Curve)
                        {
                            track.Translations.Add(new FbxImportedKeyframe<Vector3>(m_Curve.Time, new Vector3(-m_Curve.Value.X, m_Curve.Value.Y, m_Curve.Value.Z)));
                        }
                    }
                    foreach (var m_ScaleCurve in animationClip.ScaleCurves)
                    {
                        var track = iAnim.FindTrack(FixBonePath(animationClip, m_ScaleCurve.Path));
                        foreach (var m_Curve in m_ScaleCurve.Curve.Curve)
                        {
                            track.Scalings.Add(new FbxImportedKeyframe<Vector3>(m_Curve.Time, m_Curve.Value));
                        }
                    }
                    if (animationClip.EulerCurves != null)
                    {
                        foreach (var m_EulerCurve in animationClip.EulerCurves)
                        {
                            var track = iAnim.FindTrack(FixBonePath(animationClip, m_EulerCurve.Path));
                            foreach (var m_Curve in m_EulerCurve.Curve.Curve)
                            {
                                track.Rotations.Add(new FbxImportedKeyframe<Vector3>(m_Curve.Time, new Vector3(m_Curve.Value.X, -m_Curve.Value.Y, -m_Curve.Value.Z)));
                            }
                        }
                    }
                    foreach (var m_FloatCurve in animationClip.FloatCurves)
                    {
                        if (m_FloatCurve.ClassID == NativeClassID.SkinnedMeshRenderer) //BlendShape
                        {
                            var channelName = m_FloatCurve.Attribute;
                            int dotPos = channelName.IndexOf('.');
                            if (dotPos >= 0)
                            {
                                channelName = channelName.Substring(dotPos + 1);
                            }

                            var path = FixBonePath(animationClip, m_FloatCurve.Path);
                            if (string.IsNullOrEmpty(path))
                            {
                                path = GetPathByChannelName(channelName);
                            }
                            var track = iAnim.FindTrack(path);
                            track.BlendShape = new FbxImportedBlendShape();
                            track.BlendShape.ChannelName = channelName;
                            foreach (var m_Curve in m_FloatCurve.Curve.Curve)
                            {
                                track.BlendShape.Keyframes.Add(new FbxImportedKeyframe<float>(m_Curve.Time, m_Curve.Value));
                            }
                        }
                    }
                }
                else
                {
                    var m_Clip = animationClip.MuscleClip.Clip;
                    var streamedFrames = m_Clip.StreamedClip.Data;
                    var m_ClipBindingConstant = animationClip.ClipBindingConstant ?? ClipConverter.ConvertValueArrayToGenericBinding(m_Clip);
                    for (int frameIndex = 1; frameIndex < streamedFrames.Length - 1; frameIndex++)
                    {
                        var frame = streamedFrames[frameIndex];
                        var streamedValues = frame.KeyList.Select(x => x.Value).ToArray();
                        for (int curveIndex = 0; curveIndex < frame.KeyList.Length;)
                        {
                            ReadCurveData(iAnim, m_ClipBindingConstant, frame.KeyList[curveIndex].Index, 
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
                            ReadCurveData(iAnim, m_ClipBindingConstant, (int)index, time, m_DenseClip.SampleArray, (int)frameOffset, ref curveIndex);
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
                                ReadCurveData(iAnim, m_ClipBindingConstant, (int)index, time2, m_ConstantClip.Data, 0, ref curveIndex);
                            }
                            time2 = animationClip.MuscleClip.StopTime;
                        }
                    }
                }
            }
        }

        private void ReadCurveData(FbxImportedKeyframedAnimation iAnim, AnimationClipBindingConstant m_ClipBindingConstant, int index, float time, float[] data, int offset, ref int curveIndex)
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

                var bPath = FixBonePath(GetPathFromHash(binding.Path));
                if (string.IsNullOrEmpty(bPath))
                {
                    bPath = GetPathByChannelName(channelName);
                }
                var bTrack = iAnim.FindTrack(bPath);
                bTrack.BlendShape = new FbxImportedBlendShape();
                bTrack.BlendShape.ChannelName = channelName;
                bTrack.BlendShape.Keyframes.Add(new FbxImportedKeyframe<float>(time, data[curveIndex++ + offset]));
            }
            else if (binding.TypeID == NativeClassID.Transform)
            {
                var path = FixBonePath(GetPathFromHash(binding.Path));
                var track = iAnim.FindTrack(path);

                switch (binding.Attribute)
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
                boneName = AvatarConverter.FindBonePath(_avatar, hash);
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
            foreach (var pptr in m_Transform.Children)
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
            foreach (var id in _avatar.Value.AvatarSkeleton.ID)
            {
                var path = AvatarConverter.FindBonePath(_avatar, id);
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
                var skeletonPose = _avatar.Value.DefaultPose;
                var xform = skeletonPose.X[i];
                var frame = RootFrame.FindChild(transformName);
                if (frame != null)
                {
                    SetFrame(frame, xform.Translation, xform.Rotation, xform.Scale);
                }
                else
                {
                    frame = CreateFrame(transformName, xform.Translation, xform.Rotation, xform.Scale);
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
