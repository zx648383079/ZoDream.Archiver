using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class GameObjectConverter : BundleConverter<GameObject>
    {
        public override GameObject? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new GameObject();
            ReadBase(res, reader, serializer, () => {});
            return res;
        }

        public static void ReadBase(GameObject res, IBundleBinaryReader reader, IBundleSerializer serializer, Action cb)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            res.Components = reader.ReadArray(_ => {
                if (version.LessThan(5, 5)) //5.5 down
                {
                    int first = reader.ReadInt32();
                }
                return reader.ReadPPtr<Component>(serializer);
            });

            var m_Layer = reader.ReadInt32();
            cb.Invoke();
            res.Name = reader.ReadAlignedString();
           
            if (reader.TryGet<IDependencyBuilder>(out var builder))
            {
                var container = reader.Get<ISerializedFile>();
                var fileName = container.FullPath;
                var fileId = reader.Get<ObjectInfo>().FileID;
                foreach (var item in res.Components)
                {
                    builder?.AddDependencyEntry(fileName, fileId, item.PathID);
                }
            }
            
        }

        private static bool HasMesh(Transform m_Transform, List<bool> meshes)
        {
            try
            {
                m_Transform.GameObject.TryGet(out var m_GameObject);

                if (m_GameObject.MeshRenderer != null)
                {
                    var mesh = GetMesh(m_GameObject.MeshRenderer);
                    meshes.Add(mesh != null);
                }

                if (m_GameObject.SkinnedMeshRenderer != null)
                {
                    var mesh = GetMesh(m_GameObject.SkinnedMeshRenderer);
                    meshes.Add(mesh != null);
                }

                foreach (var pptr in m_Transform.Children)
                {
                    if (pptr.TryGet(out var child))
                    {
                        meshes.Add(HasMesh(child, meshes));
                    }
                }

                return meshes.Any(x => x == true);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to verify if {m_Transform?.Name} has meshes, skipping...");
                return false;
            }
        }

        private static Mesh GetMesh(Renderer meshR)
        {
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                if (sMesh.Mesh.TryGet(out var m_Mesh))
                {
                    return m_Mesh;
                }
            }
            else
            {
                if (meshR.GameObject.TryGet(out var m_GameObject) && 
                    m_GameObject.MeshFilter != null)
                {
                    if (m_GameObject.MeshFilter.Mesh.TryGet(out var m_Mesh))
                    {
                        return m_Mesh;
                    }
                }
            }

            return null;
        }
    }
}
