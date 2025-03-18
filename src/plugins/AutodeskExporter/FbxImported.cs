using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace ZoDream.AutodeskExporter
{
    public interface IFbxImported
    {
        FbxImportedFrame RootFrame { get; }
        List<FbxImportedMesh> MeshList { get; }
        List<FbxImportedMaterial> MaterialList { get; }
        List<FbxImportedTexture> TextureList { get; }
        List<FbxImportedKeyframedAnimation> AnimationList { get; }
        List<FbxImportedMorph> MorphList { get; }
    }

    public class FbxImportedFrame
    {
        public string Name { get; set; }
        public Vector3 LocalRotation { get; set; }
        public Vector3 LocalPosition { get; set; }
        public Vector3 LocalScale { get; set; }
        public FbxImportedFrame Parent { get; set; }

        private List<FbxImportedFrame> children;

        public FbxImportedFrame this[int i] => children[i];

        public int Count => children.Count;

        public string Path {
            get {
                var frame = this;
                var path = frame.Name;
                while (frame.Parent != null)
                {
                    frame = frame.Parent;
                    path = frame.Name + "/" + path;
                }
                return path;
            }
        }

        public FbxImportedFrame(int childrenCount = 0)
        {
            children = new List<FbxImportedFrame>(childrenCount);
        }

        public void AddChild(FbxImportedFrame obj)
        {
            children.Add(obj);
            obj.Parent?.Remove(obj);
            obj.Parent = this;
        }

        public void Remove(FbxImportedFrame frame)
        {
            children.Remove(frame);
        }

        public FbxImportedFrame? FindFrameByPath(string path)
        {
            var name = path.Substring(path.LastIndexOf('/') + 1);
            foreach (var frame in FindChilds(name))
            {
                if (frame.Path.EndsWith(path, StringComparison.Ordinal))
                {
                    return frame;
                }
            }
            return null;
        }

        public FbxImportedFrame? FindRelativeFrameWithPath(string path)
        {
            var subs = path.Split(['/'], 2);
            foreach (var child in children)
            {
                if (child.Name == subs[0])
                {
                    if (subs.Length == 1)
                    {
                        return child;
                    }
                    else
                    {
                        var result = child.FindRelativeFrameWithPath(subs[1]);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }

        public FbxImportedFrame? FindFrame(string name)
        {
            if (Name == name)
            {
                return this;
            }
            foreach (var child in children)
            {
                var frame = child.FindFrame(name);
                if (frame != null)
                {
                    return frame;
                }
            }
            return null;
        }

        public FbxImportedFrame? FindChild(string name, bool recursive = true)
        {
            foreach (var child in children)
            {
                if (recursive)
                {
                    var frame = child.FindFrame(name);
                    if (frame != null)
                    {
                        return frame;
                    }
                }
                else
                {
                    if (child.Name == name)
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        public IEnumerable<FbxImportedFrame> FindChilds(string name)
        {
            if (Name == name)
            {
                yield return this;
            }
            foreach (var child in children)
            {
                foreach (var item in child.FindChilds(name))
                {
                    yield return item;
                }
            }
        }
    }

    public class FbxImportedMesh
    {
        public string Path { get; set; }
        public List<FbxImportedVertex> VertexList { get; set; }
        public List<FbxImportedSubmesh> SubmeshList { get; set; }
        public List<FbxImportedBone> BoneList { get; set; }
        public bool hasNormal { get; set; }
        public bool[] hasUV { get; set; }
        public bool hasTangent { get; set; }
        public bool hasColor { get; set; }
    }

    public class FbxImportedSubmesh
    {
        public List<FbxImportedFace> FaceList { get; set; }
        public string Material { get; set; }
        public int BaseVertex { get; set; }
    }

    public class FbxImportedVertex
    {
        public Vector3 Vertex { get; set; }
        public Vector3 Normal { get; set; }
        public float[][] UV { get; set; }
        public Vector4 Tangent { get; set; }
        public Vector4 Color { get; set; }
        public float[] Weights { get; set; }
        public int[] BoneIndices { get; set; }
    }

    public class FbxImportedFace
    {
        public int[] VertexIndices { get; set; }
    }

    public class FbxImportedBone
    {
        public string Path { get; set; }
        public Matrix4x4 Matrix { get; set; }
    }

    public class FbxImportedMaterial
    {
        public string Name { get; set; }
        public Vector4 Diffuse { get; set; }
        public Vector4 Ambient { get; set; }
        public Vector4 Specular { get; set; }
        public Vector4 Emissive { get; set; }
        public Vector4 Reflection { get; set; }
        public float Shininess { get; set; }
        public float Transparency { get; set; }
        public List<FbxImportedMaterialTexture> Textures { get; set; }
    }

    public class FbxImportedMaterialTexture
    {
        public string Name { get; set; }
        public int Dest { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 Scale { get; set; }
    }

    public class FbxImportedTexture
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }

        public FbxImportedTexture(MemoryStream stream, string name)
        {
            Name = name;
            Data = stream.ToArray();
        }
        /// <summary>
        /// 不用重复导出
        /// </summary>
        /// <param name="name"></param>
        public FbxImportedTexture(string name)
        {
            Name = name;
            Data = [];
        }
    }

    public class FbxImportedKeyframedAnimation
    {
        public string Name { get; set; }
        public float SampleRate { get; set; }
        public List<FbxImportedAnimationKeyframedTrack> TrackList { get; set; }

        public FbxImportedAnimationKeyframedTrack FindTrack(string path)
        {
            var track = TrackList.Find(x => x.Path == path);
            if (track == null)
            {
                track = new FbxImportedAnimationKeyframedTrack { Path = path };
                TrackList.Add(track);
            }

            return track;
        }
    }

    public class FbxImportedAnimationKeyframedTrack
    {
        public string Path { get; set; }
        public List<FbxImportedKeyframe<Vector3>> Scalings = [];
        public List<FbxImportedKeyframe<Vector3>> Rotations = [];
        public List<FbxImportedKeyframe<Vector3>> Translations = [];
        public FbxImportedBlendShape BlendShape;
    }

    public class FbxImportedKeyframe<T>
    {
        public float time { get; set; }
        public T value { get; set; }

        public FbxImportedKeyframe(float time, T value)
        {
            this.time = time;
            this.value = value;
        }
    }

    public class FbxImportedBlendShape
    {
        public string ChannelName;
        public List<FbxImportedKeyframe<float>> Keyframes = [];
    }

    public class FbxImportedMorph
    {
        public string Path { get; set; }
        public List<FbxImportedMorphChannel> Channels { get; set; }
    }

    public class FbxImportedMorphChannel
    {
        public string Name { get; set; }
        public List<FbxImportedMorphKeyframe> KeyframeList { get; set; }
    }

    public class FbxImportedMorphKeyframe
    {
        public bool hasNormals { get; set; }
        public bool hasTangents { get; set; }
        public float Weight { get; set; }
        public List<FbxImportedMorphVertex> VertexList { get; set; }
    }

    public class FbxImportedMorphVertex
    {
        public uint Index { get; set; }
        public FbxImportedVertex Vertex { get; set; }
    }

    public static class FbxImportedHelpers
    {
        public static FbxImportedMesh? FindMesh(string path, List<FbxImportedMesh> importedMeshList)
        {
            return importedMeshList.Find(i => i.Path == path);
        }

        public static FbxImportedMaterial? FindMaterial(string name, List<FbxImportedMaterial> importedMats)
        {
            return importedMats.Find(i => i.Name == name);
        }

        public static FbxImportedTexture? FindTexture(string name, List<FbxImportedTexture> importedTextureList)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return importedTextureList.Find(i => i.Name == name);
        }
    }
}
