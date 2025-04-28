using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using UnityEngine;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using Object = UnityEngine.Object;

namespace ZoDream.BundleExtractor
{
    internal partial class UnityBundleChunkReader
    {
        private void ProcessAssets(CancellationToken token)
        {
            foreach (var asset in _assetItems)
            {
                for (var i = 0; i < asset.Count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger?.Info("Processing assets has been cancelled !!");
                        return;
                    }
                    if (asset[i] is GameObject m_GameObject)
                    {
                        foreach (var pPtr in m_GameObject.Components)
                        {
                            if (pPtr.TryGet(out var m_Component))
                            {
                                switch (m_Component)
                                {
                                    case Transform m_Transform:
                                        m_GameObject.Transform = m_Transform;
                                        TryAddExclude(pPtr.PathID);
                                        break;
                                    case MeshRenderer m_MeshRenderer:
                                        m_GameObject.MeshRenderer = m_MeshRenderer;
                                        TryAddExclude(pPtr.PathID);
                                        break;
                                    case MeshFilter m_MeshFilter:
                                        m_GameObject.MeshFilter = m_MeshFilter;
                                        TryAddExclude(pPtr.PathID);
                                        break;
                                    case SkinnedMeshRenderer m_SkinnedMeshRenderer:
                                        m_GameObject.SkinnedMeshRenderer = m_SkinnedMeshRenderer;
                                        TryAddExclude(pPtr.PathID);
                                        break;
                                    case Animator m_Animator:
                                        m_GameObject.Animator = m_Animator;
                                        TryAddExclude(pPtr.PathID);
                                        break;
                                    case Animation m_Animation:
                                        m_GameObject.Animation = m_Animation;
                                        TryAddExclude(pPtr.PathID);
                                        break;
                                }
                            }
                        }
                    }
                    else if (asset[i] is SpriteAtlas m_SpriteAtlas && m_SpriteAtlas.RenderDataMap.Count > 0)
                    {
                        foreach (var m_PackedSprite in m_SpriteAtlas.PackedSprites)
                        {
                            if (m_PackedSprite.TryGet(out var m_Sprite))
                            {
                                if (m_Sprite.SpriteAtlas.IsNull)
                                {
                                    (m_Sprite.SpriteAtlas as ObjectPPtr<SpriteAtlas>).Set(asset.Get(i), asset);
                                }
                                else if (m_Sprite.SpriteAtlas.TryGet(out var m_SpriteAtlasOld) && m_SpriteAtlasOld.IsVariant)
                                {
                                    (m_Sprite.SpriteAtlas as ObjectPPtr<SpriteAtlas>).Set(asset.Get(i), asset);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsExclude(NativeClassID type)
        {
            return type switch
            {
                NativeClassID.GameObject or NativeClassID.Mesh or NativeClassID.AnimationClip or NativeClassID.Animator => Options?.EnabledModel != true,
                NativeClassID.Shader => Options?.EnabledShader != true,
                NativeClassID.Texture2D or NativeClassID.Texture or NativeClassID.Sprite or NativeClassID.SpriteAtlas => Options?.EnabledImage != true,
                NativeClassID.AudioClip => Options?.EnabledAudio != true,
                NativeClassID.VideoClip => Options?.EnabledVideo != true,
                NativeClassID.TextAsset => Options?.EnabledLua != true && Options?.EnabledSpine != true && Options?.EnabledJson != true,
                _ => false,
            };
        }

        private void ReadAssets(CancellationToken token)
        {
            var serializer = _service.Get<IBundleSerializer>();
            foreach (var asset in _assetItems)
            {
                for (var i = 0; i < asset.Count; i ++)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger?.Info("Reading assets has been cancelled !!");
                        return;
                    }
                    var obj = asset.Get(i);
                    if (_dependency is null && IsExclude((NativeClassID)obj.ClassID))
                    {
                        continue;
                    }
                    try
                    {
                        var reader = asset.OpenRead(obj);
                        var targetType = ConvertToClassType((NativeClassID)obj.ClassID);
                        var serializedType = asset.TypeItems[obj.SerializedTypeIndex];
                        object? res = null;
                        // 默认 object 不做转化，所以为 null
                        if (serializer.Converters.TryGet(targetType, out var cvt))
                        {
                            if (serializedType.OldType is not null &&
                            serializedType.OldType.Nodes.Count > 0 &&
                            cvt is IElementTypeLoader tl)
                            {
                                res = tl.Read(reader, targetType, serializedType.OldType);
                            }
                            else
                            {
                                res = cvt.Read(reader, targetType, serializer);
                            }
                        }
                        
                        if (res is Object o)
                        {
                            asset[i] = o;
                            _dependency?.AddEntry(asset.FullPath, obj.FileID, o.Name, obj.ClassID);
                        }
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        var fullPath = BundleStorage.Separate(asset.FullPath, out var entryName);
                        var sb = new StringBuilder();
                        sb.AppendLine("Unable to load object")
                            .AppendLine($"Assets {entryName}")
                            .AppendLine($"Path {fullPath}")
                            .AppendLine($"Type {obj.TypeID}")
                            .AppendLine($"PathID {obj.FileID}")
                            .Append(e);
                        Debug.WriteLine(sb.ToString());
#endif
                        //Logger.Error(sb.ToString());
                        //Logger.Error("Unable to load object");
                        Logger?.Error(e.Message);
                    }
                }
            }
        }

        private static Type ConvertToClassType(NativeClassID classID)
        {
            return classID switch
            {
                NativeClassID.Animation => typeof(Animation),
                NativeClassID.AnimationClip => typeof(AnimationClip),
                NativeClassID.Animator => typeof(Animator),
                NativeClassID.AnimatorController => typeof(AnimatorController),
                NativeClassID.AnimatorOverrideController => typeof(AnimatorOverrideController),
                NativeClassID.AssetBundle => typeof(AssetBundle),
                NativeClassID.AudioClip => typeof(AudioClip),
                NativeClassID.Avatar => typeof(Avatar),
                NativeClassID.Font => typeof(Font),
                NativeClassID.GameObject => typeof(GameObject),
                NativeClassID.IndexObject => typeof(IndexObject),
                NativeClassID.Material => typeof(Material),
                NativeClassID.Mesh => typeof(Mesh),
                NativeClassID.MeshFilter => typeof(MeshFilter),
                NativeClassID.MeshRenderer => typeof(MeshRenderer),
                NativeClassID.MiHoYoBinData => typeof(MiHoYoBinData),
                NativeClassID.MonoBehaviour => typeof(MonoBehaviour),
                NativeClassID.MonoScript => typeof(MonoScript),
                NativeClassID.MovieTexture => typeof(MovieTexture),
                NativeClassID.PlayerSettings => typeof(PlayerSettings),
                NativeClassID.RectTransform => typeof(RectTransform),
                NativeClassID.Shader => typeof(Shader),
                NativeClassID.SkinnedMeshRenderer => typeof(SkinnedMeshRenderer),
                NativeClassID.Sprite => typeof(Sprite),
                NativeClassID.SpriteAtlas => typeof(SpriteAtlas),
                NativeClassID.TextAsset => typeof(TextAsset),
                NativeClassID.Texture2D => typeof(Texture2D),
                NativeClassID.Texture2DArray => typeof(Texture2D),
                NativeClassID.Transform => typeof(Transform),
                NativeClassID.VideoClip => typeof(VideoClip),
                NativeClassID.ResourceManager => typeof(ResourceManager),
                _ => typeof(Object),
            };
        }

        public T? ConvertTo<T>(ISerializedFile asset, ObjectInfo obj)
        {
            var serializer = _service.Get<IBundleSerializer>();
            var reader = asset.OpenRead(obj);
            var targetType = typeof(T);//ConvertToClassType((NativeClassID)obj.ClassID);
            var serializedType = asset.TypeItems[obj.SerializedTypeIndex];
            if (serializer.Converters.TryGet(targetType, out var cvt))
            {
                if (serializedType.OldType is not null &&
                serializedType.OldType.Nodes.Count > 0 &&
                cvt is IElementTypeLoader tl)
                {
                    return (T)tl.Read(reader, targetType, serializedType.OldType);
                }
                else
                {
                    return (T)cvt.Read(reader, targetType, serializer);
                }
            }
            return default;
        }
    }
}
