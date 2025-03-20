using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace ZoDream.AutodeskExporter
{
    public partial class FbxContext : IDisposable
    {

        private readonly FbxManager _manager = new();
        private FbxScene? _scene;
        private FbxExporter? _exporter;
        private FbxPose? _pose;
        private string[] _framePaths = [];


        private readonly Dictionary<FbxImportedFrame, FbxNode> _frameToNode = [];
        private readonly List<KeyValuePair<string, FbxSurfacePhong>> _createdMaterials = [];
        private readonly Dictionary<string, FbxFileTexture> _createdTextures = [];
        public bool IsDisposed { get; private set; }

        private void EnsureNotDisposed()
        {
            if (!IsDisposed)
            {
                return;
            }
            throw new ObjectDisposedException(nameof(FbxContext));
        }

        internal void Initialize(string fileName, float scaleFactor, bool is60Fps)
        {
            EnsureNotDisposed();
            var setting = new FbxIOSettings(_manager, FbxIOSettings.IOSROOT);
            _manager.IOSettings = setting;
            _scene = new FbxScene(_manager, "game_scene");

            setting.SetBoolProp(FbxIOSettings.EXP_FBX_MATERIAL, true);
            setting.SetBoolProp(FbxIOSettings.EXP_FBX_TEXTURE, true);
            setting.SetBoolProp(FbxIOSettings.EXP_FBX_EMBEDDED, true);
            setting.SetBoolProp(FbxIOSettings.EXP_FBX_SHAPE, true);
            setting.SetBoolProp(FbxIOSettings.EXP_FBX_GOBO, true);
            setting.SetBoolProp(FbxIOSettings.EXP_FBX_ANIMATION, true);
            setting.SetBoolProp(FbxIOSettings.EXP_FBX_GLOBAL_SETTINGS, true);

            var globalSetting = _scene.GlobalSettings;
            globalSetting.SetSystemUnit(new FbxSystemUnit(scaleFactor));
            if (is60Fps)
            {
                globalSetting.SetTimeMode(EMode.eFrames60);
            }
            _exporter = new FbxExporter(_manager, string.Empty);
            _exporter.SetFileExportVersion(FbxVersion.FBX202000);
            if (!_exporter.Initialize(fileName, 0, setting))
            {
                throw new ApplicationException($"Failed to initialize FbxExporter: ");
            }
            _pose = new FbxPose(_manager, "BindPose");
            _scene.AddPose(_pose);
        }

        internal void SetFramePaths(HashSet<string> framePaths)
        {
            EnsureNotDisposed();

            if (framePaths == null || framePaths.Count == 0)
            {
                return;
            }

            _framePaths = [.. framePaths];
        }

        internal void ExportScene()
        {
            EnsureNotDisposed();

            if (_scene is not null)
            {
                _exporter?.Export(_scene);
            }
        }

        internal void ExportFrame(List<FbxImportedMesh> meshList, List<FbxImportedFrame> meshFrames, FbxImportedFrame rootFrame)
        {
            var rootNode = _scene?.RootNode;
            Debug.Assert(rootNode is not null);

            var nodeStack = new Stack<FbxNode>();
            var frameStack = new Stack<FbxImportedFrame>();

            nodeStack.Push(rootNode);
            frameStack.Push(rootFrame);

            while (nodeStack.Count > 0)
            {
                var parentNode = nodeStack.Pop();
                var frame = frameStack.Pop();

                var childNode = ExportSingleFrame(parentNode, frame.Path, frame.Name, frame.LocalPosition, frame.LocalRotation, frame.LocalScale);

                if (meshList != null && FbxImportedHelpers.FindMesh(frame.Path, meshList) != null)
                {
                    meshFrames.Add(frame);
                }

                _frameToNode.Add(frame, childNode);

                for (var i = frame.Count - 1; i >= 0; i -= 1)
                {
                    nodeStack.Push(childNode);
                    frameStack.Push(frame[i]);
                }
            }
        }

        private FbxNode? ExportSingleFrame(FbxNode parentNode, string path, string name,
            Vector3 localPosition, Vector3 localRotation, Vector3 localScale)
        {
            if (_scene is null)
            {
                return null;
            }
            if (_framePaths.Length != 0 && !_framePaths.Contains(path))
            {
                return null;
            }
            var node = new FbxNode(_scene, name);
            node.LclScaling = localScale;
            node.LclRotation = localRotation;
            node.LclTranslation = localPosition;
            node.SetPreferedAngle(new(localRotation, 1));

            parentNode.AddChild(node);
            _pose?.Add(node, new FbxMatrix(node.EvaluateGlobalTransform()));
            return node;
        }

        internal void SetJointsNode(FbxImportedFrame rootFrame, HashSet<string> bonePaths, bool castToBone, float boneSize)
        {
            var frameStack = new Stack<FbxImportedFrame>();

            frameStack.Push(rootFrame);

            while (frameStack.Count > 0)
            {
                var frame = frameStack.Pop();

                if (_frameToNode.TryGetValue(frame, out var node))
                {
                    Debug.Assert(node is not null);

                    if (castToBone)
                    {
                        if (_scene is not null && node is not null)
                        {
                            var pJoint = new FbxSkeleton(_scene, string.Empty);
                            pJoint.Size = boneSize;
                            pJoint.SetSkeletonType(FbxSkeleton.EType.eLimbNode);
                            node.SetNodeAttribute(pJoint);
                        }
                    }
                    else
                    {
                        Debug.Assert(bonePaths != null);

                        if (bonePaths.Contains(frame.Path))
                        {
                            var pJoint = new FbxSkeleton(_scene, string.Empty);
                            pJoint.Size = boneSize;
                            pJoint.SetSkeletonType(FbxSkeleton.EType.eLimbNode);
                            node.SetNodeAttribute(pJoint);

                            pJoint = new FbxSkeleton(_scene, string.Empty);
                            pJoint.Size = boneSize;
                            pJoint.SetSkeletonType(FbxSkeleton.EType.eLimbNode);
                            node.GetParent()?.SetNodeAttribute(pJoint);
                        }
                        else
                        {
                            var pNull = new FbxNull(_scene, string.Empty);
                            if (node.ChildCount > 0)
                            {
                                pNull.Look = FbxNull.ELook.eNone;
                            }
                            node.SetNodeAttribute(pNull);
                        }
                    }
                }

                for (var i = frame.Count - 1; i >= 0; i -= 1)
                {
                    frameStack.Push(frame[i]);
                }
            }
        }

        internal void PrepareMaterials(int materialCount, int textureCount)
        {
            //_pMaterials = new FbxSurfacePhong[materialCount];
            //_pTextures = new FbxFileTexture[textureCount];
        }

        internal void ExportMeshFromFrame(FbxImportedFrame rootFrame, 
            FbxImportedFrame meshFrame, List<FbxImportedMesh> meshList, 
            List<FbxImportedMaterial> materialList, List<FbxImportedTexture> textureList, 
            bool exportSkins, bool exportAllUvsAsDiffuseMaps)
        {
            var meshNode = _frameToNode[meshFrame];
            var mesh = FbxImportedHelpers.FindMesh(meshFrame.Path, meshList);

            ExportMesh(rootFrame, materialList, textureList, meshNode, mesh, exportSkins, exportAllUvsAsDiffuseMaps);
        }

        private FbxFileTexture? ExportTexture(FbxImportedTexture texture)
        {
            if (texture == null)
            {
                return null;
            }

            if (_createdTextures.TryGetValue(texture.Name, out FbxFileTexture? value))
            {
                return value;
            }

            var pTex = new FbxFileTexture(_manager, texture.Name);
            pTex.FileName = texture.Name;
            pTex.SetTextureUse(FbxFileTexture.ETextureUse.eStandard);
            pTex.SetMappingType(FbxFileTexture.EMappingType.eUV);
            pTex.MaterialUse = FbxFileTexture.EMaterialUse.eModelMaterial;
            pTex.SetSwapUV(false);
            pTex.SetTranslation(0.0, 0.0);
            pTex.SetScale(1.0, 1.0);
            pTex.SetRotation(0.0, 0.0);
            _createdTextures.Add(texture.Name, pTex);

            //var file = new FileInfo(texture.Name);

            //using (var writer = new BinaryWriter(file.Create()))
            //{
            //    writer.Write(texture.Data);
            //}

            return pTex;
        }

        private void ExportMesh(FbxImportedFrame rootFrame, 
            List<FbxImportedMaterial> materialList, 
            List<FbxImportedTexture> textureList, FbxNode frameNode, 
            FbxImportedMesh importedMesh, bool exportSkins, bool exportAllUvsAsDiffuseMaps)
        {
            var boneList = importedMesh.BoneList;
            var totalBoneCount = 0;
            var hasBones = false;
            if (exportSkins && boneList?.Count > 0)
            {
                totalBoneCount = boneList.Count;
                hasBones = true;
            }

            FbxArray<FbxCluster> pClusterArray = null;

            try
            {
                if (hasBones)
                {
                    pClusterArray = new FbxArray<FbxCluster>(totalBoneCount);

                    foreach (var bone in boneList)
                    {
                        if (bone.Path != null)
                        {
                            var frame = rootFrame.FindFrameByPath(bone.Path);
                            var boneNode = _frameToNode[frame];

                            var cluster = new FbxCluster(_scene, boneNode.Name + "Cluster");
                            cluster.SetLink(boneNode);
                            cluster.SetLinkMode(FbxCluster.ELinkMode.eTotalOne);

                            pClusterArray.Add(cluster);
                        }
                        else
                        {
                            pClusterArray.Add(new FbxCluster(nint.Zero));
                        }
                    }
                }

                var mesh = new FbxMesh(_scene, frameNode.Name);
                frameNode.SetNodeAttribute(mesh);

                mesh.InitControlPoints(importedMesh.VertexList.Count);

                if (importedMesh.hasNormal)
                {
                    var puv = mesh.CreateElementNormal();
                    puv.MappingMode = EMappingMode.eByControlPoint;
                    puv.ReferenceMode = EReferenceMode.eDirect;
                }

                //var uvLayerItems = new Dictionary<int, FbxLayerElementUV>();

                for (int i = 0; i < importedMesh.hasUV.Length; i++)
                {
                    if (!importedMesh.hasUV[i]) 
                    { 
                        continue;
                    }

                    if (i == 1 && !exportAllUvsAsDiffuseMaps)
                    {
                        var puv = mesh.CreateElementUV($"UV{i}", FbxLayerElement.EType.eTextureNormalMap);
                        puv.MappingMode = EMappingMode.eByControlPoint;
                        puv.ReferenceMode = EReferenceMode.eDirect;
                        //uvLayerItems.Add(i, puv);
                    }
                    else
                    {
                        var puv = mesh.CreateElementUV($"UV{i}", FbxLayerElement.EType.eTextureDiffuse);
                        puv.MappingMode = EMappingMode.eByControlPoint;
                        puv.ReferenceMode = EReferenceMode.eDirect;
                        //uvLayerItems.Add(i, puv);
                    }
                }

                

                if (importedMesh.hasTangent)
                {
                    var pTangent = mesh.CreateElementTangent();
                    pTangent.MappingMode = EMappingMode.eByControlPoint;
                    pTangent.ReferenceMode = EReferenceMode.eDirect;
                }

                if (importedMesh.hasColor)
                {
                    var pVertexColor = mesh.CreateElementVertexColor();
                    pVertexColor.MappingMode = EMappingMode.eByControlPoint;
                    pVertexColor.ReferenceMode = EReferenceMode.eDirect;
                }

                var pMaterial = mesh.CreateElementMaterial();
                pMaterial.MappingMode = EMappingMode.eByPolygon;
                pMaterial.ReferenceMode = EReferenceMode.eIndexToDirect;

                foreach (var meshObj in importedMesh.SubmeshList)
                {
                    var materialIndex = 0;
                    var mat = FbxImportedHelpers.FindMaterial(meshObj.Material, materialList);

                    if (mat != null)
                    {
                        var foundMat = _createdMaterials.FindIndex(kv => kv.Key == mat.Name);
                        FbxSurfacePhong pMat;

                        if (foundMat >= 0)
                        {
                            pMat = _createdMaterials[foundMat].Value;
                        }
                        else
                        {
                            var diffuse = mat.Diffuse;
                            var ambient = mat.Ambient;
                            var emissive = mat.Emissive;
                            var specular = mat.Specular;
                            var reflection = mat.Reflection;

                            pMat = new(_manager, mat.Name);
                            pMat.Diffuse = new Vector3(diffuse.X, diffuse.Y, diffuse.Z);
                            pMat.Ambient = new Vector3(ambient.X, ambient.Y, ambient.Z);
                            pMat.Emissive = new Vector3(emissive.X, emissive.Y, emissive.Z);
                            pMat.Specular = new Vector3(specular.X, specular.Y, specular.Z);
                            pMat.Reflection = new Vector3(reflection.X, reflection.Y, reflection.Z);
                            pMat.Shininess = mat.Shininess;
                            pMat.TransparencyFactor = mat.Transparency;
                            pMat.ShadingModel = "Phong";
                            _createdMaterials.Add(new(mat.Name, pMat));
                        }

                        materialIndex = frameNode.AddMaterial(pMat);

                        var hasTexture = false;

                        foreach (var texture in mat.Textures)
                        {
                            var tex = FbxImportedHelpers.FindTexture(texture.Name, textureList);
                            var pTexture = ExportTexture(tex);

                            if (pTexture is not null && texture.Dest < 4)
                            {
                                hasTexture = true;
                                pTexture.SetTranslation(texture.Offset.X, texture.Offset.Y);
                                pTexture.SetScale(texture.Scale.X, texture.Scale.Y);

                                switch (texture.Dest)
                                {
                                    case 0:
                                        pMat.DiffuseConnectSrcObject(pTexture);
                                        break;
                                    case 1:
                                        pMat.NormalMapConnectSrcObject(pTexture);
                                        break;
                                    case 2:
                                        pMat.SpecularConnectSrcObject(pTexture);
                                        break;
                                    case 3:
                                        pMat.BumpConnectSrcObject(pTexture);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        if (hasTexture)
                        {
                            frameNode.SetShadingMode(FbxNode.EShadingMode.eTextureShading);
                        }
                    }

                    foreach (var face in meshObj.FaceList)
                    {
                        var index0 = face.VertexIndices[0] + meshObj.BaseVertex;
                        var index1 = face.VertexIndices[1] + meshObj.BaseVertex;
                        var index2 = face.VertexIndices[2] + meshObj.BaseVertex;

                        mesh.BeginPolygon(materialIndex);
                        mesh.AddPolygon(index0);
                        mesh.AddPolygon(index1);
                        mesh.AddPolygon(index2);
                        mesh.EndPolygon();
                    }
                }

                var vertexList = importedMesh.VertexList;

                var vertexCount = vertexList.Count;

                for (var j = 0; j < vertexCount; j += 1)
                {
                    var importedVertex = vertexList[j];

                    var vertex = importedVertex.Vertex;
                    mesh.SetControlPointAt(j, new Vector4(vertex.X, vertex.Y, vertex.Z, 0));

                    if (importedMesh.hasNormal)
                    {
                        var normal = importedVertex.Normal;
                        mesh.GetElementNormal(0).AddDirect(normal.X, normal.Y, normal.Z, 0);
                    }

                    for (var uvIndex = 0; uvIndex < importedMesh.hasUV.Length; uvIndex += 1)
                    {
                        if (importedMesh.hasUV[uvIndex])
                        {
                            var uv = importedVertex.UV[uvIndex];
                            var puv = mesh.GetElementUV($"UV{uvIndex}");// uvLayerItems[uvIndex];
                            puv.AddDirect(uv[0], uv[1]);
                        }
                    }

                    if (importedMesh.hasTangent)
                    {
                        var tangent = importedVertex.Tangent;
                        mesh.GetElementTangent(0).AddDirect(tangent.X, tangent.Y, tangent.Z, tangent.W);
                    }

                    if (importedMesh.hasColor)
                    {
                        var color = importedVertex.Color;
                        mesh.GetElementVertexColor(0).AddDirect(color.X, color.Y, color.Z, color.W);
                    }

                    if (hasBones && importedVertex.BoneIndices != null)
                    {
                        var boneIndices = importedVertex.BoneIndices;
                        var boneWeights = importedVertex.Weights;

                        for (var k = 0; k < 4; k += 1)
                        {
                            if (boneIndices[k] < totalBoneCount && boneWeights[k] > 0)
                            {
                                pClusterArray.GetAt(boneIndices[k])?.AddControlPointIndex(j, boneWeights[k]);
                            }
                        }
                    }
                }


                if (hasBones)
                {

                    using var pSkin = new FbxSkin(_scene, string.Empty);
                    var lMeshMatrix = frameNode.EvaluateGlobalTransform();
                    for (var j = 0; j < totalBoneCount; j += 1)
                    {
                        var pCluster = pClusterArray.GetAt(j);
                        if (pCluster is null)
                        {
                            continue;
                        }

                        var m = boneList[j].Matrix;

                        // CopyMatrix4x4(in m, boneMatrix);
                        var boneMatrix = new FbxAMatrix(m);
                        pCluster.SetTransformMatrix(lMeshMatrix);
                        pCluster.SetTransformLinkMatrix(lMeshMatrix * boneMatrix.Inverse());
                        pSkin.AddCluster(pCluster);
                    }
                    if (pSkin.ClusterCount > 0)
                    {
                        mesh.AddDeformer(pSkin);
                    }
                }
            }
            finally
            {
                pClusterArray?.Dispose();
            }
        }

        internal void ExportAnimations(FbxImportedFrame rootFrame, List<FbxImportedKeyframedAnimation> animationList, bool eulerFilter, float filterPrecision)
        {
            if (animationList == null || animationList.Count == 0)
            {
                return;
            }

            var lFilter = eulerFilter ? new FbxAnimCurveFilterUnroll() : null;

            for (int i = 0; i < animationList.Count; i++)
            {
                var importedAnimation = animationList[i];
                string takeName;

                if (importedAnimation.Name != null)
                {
                    takeName = importedAnimation.Name;
                }
                else
                {
                    takeName = $"Take{i}";
                }
                var lAnimStack = new FbxAnimStack(_scene, takeName);
                var lAnimLayer = new FbxAnimLayer(_scene, "Base Layer");
                lAnimStack.AddMember(lAnimLayer);

                ExportKeyframedAnimation(rootFrame, importedAnimation,
                    lFilter,
                    lAnimStack,
                    lAnimLayer,
                    filterPrecision);
            }
        }

        private void ExportKeyframedAnimation(FbxImportedFrame rootFrame, 
            FbxImportedKeyframedAnimation parser,
            FbxAnimCurveFilterUnroll eulerFilter,
            FbxAnimStack lAnimStack, 
            FbxAnimLayer lAnimLayer, float filterPrecision)
        {
            foreach (var track in parser.TrackList)
            {
                if (track.Path == null)
                {
                    continue;
                }

                var frame = rootFrame.FindFrameByPath(track.Path);

                if (frame == null)
                {
                    continue;
                }

                var pNode = _frameToNode[frame];

                var lCurveSX = pNode.LclScalingGetCurve(lAnimLayer, "X", true);
                var lCurveSY = pNode.LclScalingGetCurve(lAnimLayer, "Y", true);
                var lCurveSZ = pNode.LclScalingGetCurve(lAnimLayer, "Z", true);
                var lCurveRX = pNode.LclRotationGetCurve(lAnimLayer, "X", true);
                var lCurveRY = pNode.LclRotationGetCurve(lAnimLayer, "Y", true);
                var lCurveRZ = pNode.LclRotationGetCurve(lAnimLayer, "Z", true);
                var lCurveTX = pNode.LclTranslationGetCurve(lAnimLayer, "X", true);
                var lCurveTY = pNode.LclTranslationGetCurve(lAnimLayer, "Y", true);
                var lCurveTZ = pNode.LclTranslationGetCurve(lAnimLayer, "Z", true);

                lCurveSX.KeyModifyBegin();
                lCurveSY.KeyModifyBegin();
                lCurveSZ.KeyModifyBegin();
                lCurveRX.KeyModifyBegin();
                lCurveRY.KeyModifyBegin();
                lCurveRZ.KeyModifyBegin();
                lCurveTX.KeyModifyBegin();
                lCurveTY.KeyModifyBegin();
                lCurveTZ.KeyModifyBegin();

                foreach (var scaling in track.Scalings)
                {
                    var value = scaling.value;
                    var lTime = new FbxTime(scaling.time);
                    lCurveSX.KeySet(lCurveSX.KeyAdd(lTime), lTime, value.X);
                    lCurveSY.KeySet(lCurveSY.KeyAdd(lTime), lTime, value.Y);
                    lCurveSZ.KeySet(lCurveSZ.KeyAdd(lTime), lTime, value.Z);
                }

                foreach (var rotation in track.Rotations)
                {
                    var value = rotation.value;
                    var lTime = new FbxTime(rotation.time);
                    lCurveRX.KeySet(lCurveRX.KeyAdd(lTime), lTime, value.X);
                    lCurveRY.KeySet(lCurveRY.KeyAdd(lTime), lTime, value.Y);
                    lCurveRZ.KeySet(lCurveRZ.KeyAdd(lTime), lTime, value.Z);
                }

                foreach (var translation in track.Translations)
                {
                    var value = translation.value;
                    var lTime = new FbxTime(translation.time);
                    lCurveTX.KeySet(lCurveTX.KeyAdd(lTime), lTime, value.X);
                    lCurveTY.KeySet(lCurveTY.KeyAdd(lTime), lTime, value.Y);
                    lCurveTZ.KeySet(lCurveTZ.KeyAdd(lTime), lTime, value.Z);
                }

                lCurveSX.KeyModifyEnd();
                lCurveSY.KeyModifyEnd();
                lCurveSZ.KeyModifyEnd();
                lCurveRX.KeyModifyEnd();
                lCurveRY.KeyModifyEnd();
                lCurveRZ.KeyModifyEnd();
                lCurveTX.KeyModifyEnd();
                lCurveTY.KeyModifyEnd();
                lCurveTZ.KeyModifyEnd();


                eulerFilter.Reset();
                eulerFilter.SetQualityTolerance(filterPrecision);
                eulerFilter.Apply([lCurveRX, lCurveRY, lCurveRZ]);

                var blendShape = track.BlendShape;

                if (blendShape != null)
                {
                    var pMesh = pNode.GetMesh();
                    var lBlendShape = pMesh.GetDeformerCount(FbxDeformer.EDeformerType.eBlendShape) >= 0
                        ? pMesh.GetDeformer<FbxBlendShape>(0, FbxDeformer.EDeformerType.eBlendShape) : null;
                    var channelCount = lBlendShape?.BlendShapeChannelCount ?? 0;

                    if (channelCount > 0)
                    {
                        for (var channelIndex = 0; channelIndex < channelCount; channelIndex += 1)
                        {
                            
                            if (lBlendShape.GetBlendShapeChannel(channelIndex).Name != blendShape.ChannelName)
                            {
                                continue;
                            }

                            var lAnimCurve = pMesh.GetShapeChannel(0, channelIndex, lAnimLayer, true);
                            lAnimCurve.KeyModifyBegin();

                            foreach (var keyframe in blendShape.Keyframes)
                            {
                                var lTime = new FbxTime(keyframe.time);

                                var keyIndex = lAnimCurve.KeyAdd(lTime);
                                lAnimCurve.KeySetValue(keyIndex, keyframe.value);
                                lAnimCurve.KeySetInterpolation(keyIndex, FbxAnimCurve.EInterpolationType.eInterpolationCubic);
                            }

                            lAnimCurve.KeyModifyEnd();
                        }
                    }
                }
            }
        }

        internal void ExportMorphs(FbxImportedFrame rootFrame, 
            List<FbxImportedMorph> morphList)
        {
            if (morphList == null || morphList.Count == 0)
            {
                return;
            }

            foreach (var morph in morphList)
            {
                var frame = rootFrame.FindFrameByPath(morph.Path);

                if (frame == null)
                {
                    continue;
                }

                var pNode = _frameToNode[frame];

                var pMesh = pNode.GetMesh();
                var lBlendShape = new FbxBlendShape(_scene, $"{pMesh.Name}BlendShape");
                pMesh.AddDeformer(lBlendShape);

                foreach (var channel in morph.Channels)
                {
                    var lBlendShapeChannel = new FbxBlendShapeChannel(_scene, channel.Name);


                    lBlendShape.AddBlendShapeChannel(lBlendShapeChannel);

                    for (var i = 0; i < channel.KeyframeList.Count; i++)
                    {
                        var keyframe = channel.KeyframeList[i];

                        var lShape = new FbxShape(_scene, i == 0 ? channel.Name : $"{channel.Name}_{i + 1}");
                        lBlendShapeChannel.AddTargetShape(lShape, keyframe.Weight);

                        var vectorCount = pMesh.ControlPointsCount;

                        lShape.InitControlPoints(vectorCount);

                        for (int j = 0; j < vectorCount; j++)
                        {
                            lShape.SetControlPointAt(j, pMesh.GetControlPointAt(j)); ;
                        }

                        foreach (var vertex in keyframe.VertexList)
                        {
                            var v = vertex.Vertex.Vertex;
                            lShape.SetControlPointAt((int)vertex.Index, new Vector4(v.X, v.Y, v.Z, 0));
                        }

                        if (keyframe.hasNormals)
                        {
                            lShape.InitNormals(pMesh);

                            foreach (var vertex in keyframe.VertexList)
                            {
                                var v = vertex.Vertex.Normal;
                                lShape.SetControlPointNormalAt((int)vertex.Index, new Vector4(v.X, v.Y, v.Z, 0));
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var item in _frameToNode)
            {
                item.Value.Dispose();
            }
            _frameToNode.Clear();
            _createdMaterials.Clear();
            _createdTextures.Clear();

            // _pose?.Dispose();
            // _scene?.Dispose();
            _exporter?.Dispose();
            // _setting?.Dispose();
            _manager.Dispose();
        }
    }
}
