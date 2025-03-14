using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ZoDream.AutodeskExporter
{
    public partial class FbxContext(string fileName, IFbxImported imported, bool allNodes, bool exportSkins, bool castToBone, float boneSize, bool exportAllUvsAsDiffuseMaps, float scaleFactor, int versionIndex, bool isAscii)
    {

        public void Initialize()
        {
            var is60Fps = imported.AnimationList.Count > 0 && imported.AnimationList[0].SampleRate.Equals(60.0f);

            Initialize(fileName, scaleFactor, versionIndex, isAscii, is60Fps);

            if (!allNodes)
            {
                var framePaths = SearchHierarchy();

                SetFramePaths(framePaths);
            }
        }

        public void ExportAll(bool blendShape, bool animation, bool eulerFilter, float filterPrecision)
        {
            var meshFrames = new List<FbxImportedFrame>();

            ExportRootFrame(meshFrames);

            if (imported.MeshList != null)
            {
                SetJointsFromImportedMeshes();

                PrepareMaterials();

                ExportMeshFrames(imported.RootFrame, meshFrames);
            }
            else
            {
                SetJointsNode(imported.RootFrame, null, true);
            }



            if (blendShape)
            {
                ExportMorphs();
            }

            if (animation)
            {
                ExportAnimations(eulerFilter, filterPrecision);
            }

            ExportScene();
        }

        private void ExportMorphs()
        {
            ExportMorphs(imported.RootFrame, imported.MorphList);
        }

        private void ExportAnimations(bool eulerFilter, float filterPrecision)
        {
            ExportAnimations(imported.RootFrame, imported.AnimationList, eulerFilter, filterPrecision);
        }

        private void ExportRootFrame(List<FbxImportedFrame> meshFrames)
        {
            ExportFrame(imported.MeshList, meshFrames, imported.RootFrame);
        }

        private void SetJointsFromImportedMeshes()
        {
            if (!exportSkins)
            {
                return;
            }

            Debug.Assert(imported.MeshList != null);

            var bonePaths = new HashSet<string>();

            foreach (var mesh in imported.MeshList)
            {
                var boneList = mesh.BoneList;

                if (boneList != null)
                {
                    foreach (var bone in boneList)
                    {
                        bonePaths.Add(bone.Path);
                    }
                }
            }

            SetJointsNode(imported.RootFrame, bonePaths, castToBone);
        }

        private void SetJointsNode(FbxImportedFrame rootFrame, HashSet<string> bonePaths, bool castToBone)
        {
            SetJointsNode(rootFrame, bonePaths, castToBone, boneSize);
        }

        private void PrepareMaterials()
        {
            PrepareMaterials(imported.MaterialList.Count, imported.TextureList.Count);
        }

        private void ExportMeshFrames(FbxImportedFrame rootFrame, List<FbxImportedFrame> meshFrames)
        {
            foreach (var meshFrame in meshFrames)
            {
                ExportMeshFromFrame(rootFrame, meshFrame, imported.MeshList, imported.MaterialList, 
                    imported.TextureList, exportSkins, exportAllUvsAsDiffuseMaps);
            }
        }

        private HashSet<string> SearchHierarchy()
        {
            if (imported.MeshList == null || imported.MeshList.Count == 0)
            {
                return null;
            }

            var exportFrames = new HashSet<string>();

            SearchHierarchy(imported.RootFrame, imported.MeshList, exportFrames);

            return exportFrames;
        }

        private static void SearchHierarchy(FbxImportedFrame rootFrame, List<FbxImportedMesh> meshList, HashSet<string> exportFrames)
        {
            var frameStack = new Stack<FbxImportedFrame>();

            frameStack.Push(rootFrame);

            while (frameStack.Count > 0)
            {
                var frame = frameStack.Pop();

                var meshListSome = FbxImportedHelpers.FindMesh(frame.Path, meshList);

                if (meshListSome != null)
                {
                    var parent = frame;

                    while (parent != null)
                    {
                        exportFrames.Add(parent.Path);
                        parent = parent.Parent;
                    }

                    var boneList = meshListSome.BoneList;

                    if (boneList != null)
                    {
                        foreach (var bone in boneList)
                        {
                            if (!exportFrames.Contains(bone.Path))
                            {
                                var boneParent = rootFrame.FindFrameByPath(bone.Path);

                                while (boneParent != null)
                                {
                                    exportFrames.Add(boneParent.Path);
                                    boneParent = boneParent.Parent;
                                }
                            }
                        }
                    }
                }

                for (var i = frame.Count - 1; i >= 0; i -= 1)
                {
                    frameStack.Push(frame[i]);
                }
            }
        }

    }
}
