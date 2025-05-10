using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

        public static bool TryGet<T>(GameObject game, [NotNullWhen(true)] out T? result)
            where T : Component
        {
            foreach (var item in ForEach<T>(game))
            {
                result = item;
                return true;
            }
            result = null;
            return false;
        }

        public static IEnumerable<T> ForEach<T>(GameObject game)
            where T : Component
        {
            foreach (var item in game.Components)
            {
                if (item.TryGet(out var res) && res is T instance)
                {
                    yield return instance;
                }
            }
        }

        private static bool HasMesh(Transform transform, List<bool> meshes)
        {
            try
            {
                if (transform.GameObject?.TryGet(out var game) == true)
                {
                    foreach (var item in ForEach<Component>(game))
                    {
                        if (item is Renderer r && GetMesh(r) is not null)
                        {
                            return true;
                        }
                    }
                }

                foreach (var pptr in transform.Children)
                {
                    if (pptr.TryGet(out var child) && HasMesh(child, meshes))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                Debug.WriteLine($"Unable to verify if {transform?.Name} has meshes, skipping...");
                return false;
            }
        }

        public static Mesh? GetMesh(Renderer meshR)
        {
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                if (sMesh.Mesh.TryGet(out var mesh))
                {
                    return mesh;
                }
            }
            else if(true == meshR.GameObject?.TryGet(out var game) && 
                TryGet<MeshFilter>(game, out var filter) && filter.Mesh.TryGet(out var mesh))
            {
                return mesh;
            }
            return null;
        }
        /// <summary>
        /// 判断是否根节点
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool IsRoot(GameObject game)
        {
            return TryGet<Transform>(game, out var transform) && TransformConverter.IsRoot(transform);
        }
    }
}
