using System;
using System.Collections.Generic;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class TransformConverter : BundleConverter<Transform>
    {
        public override Transform? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new Transform
            {
                GameObject = reader.ReadPPtr<GameObject>(serializer),
                LocalRotation = reader.ReadQuaternion(),
                LocalPosition = reader.ReadVector3Or4(),
                LocalScale = reader.ReadVector3Or4(),

                Children = reader.ReadPPtrArray<Transform>(serializer),
                Father = reader.ReadPPtr<Transform>(serializer)
            };

            return res;
        }
        /// <summary>
        /// 是否是根节点
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static bool IsRoot(Transform transform)
        {
            return !transform.Father.TryGet(out _);
        }

        /// <summary>
        /// 获取根节点
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Transform GetRoot(Transform transform)
        {
            while (transform.Father.TryGet(out var next))
            {
                transform = next;
            }
            return transform;
        }
        
        /// <summary>
        /// 遍历子节点
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IEnumerable<Transform> ForEach(Transform transform)
        {
            foreach (var item in transform.Children)
            {
                if (item.TryGet(out var res))
                {
                    yield return res;
                }
            }
        }

        /// <summary>
        /// 遍历子孙节点
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IEnumerable<Transform> ForEachDeep(Transform transform)
        {
            var from = transform.Children;
            var next = new List<IPPtr<Transform>>();
            while (from.Length > 0)
            {
                foreach (var item in from)
                {
                    if (!item.TryGet(out var instance))
                    {
                        continue;
                    }
                    yield return instance;
                    next.AddRange(instance.Children);
                }
                from = [.. next];
                next.Clear();
            }
        }

        public static IEnumerable<Transform> ForEachTree(Transform transform)
        {
            yield return transform;
            foreach (var item in ForEachDeep(transform))
            {
                yield return item;
            }
        }
    }
}
