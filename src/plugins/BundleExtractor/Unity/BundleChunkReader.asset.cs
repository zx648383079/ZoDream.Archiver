using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Logging;
using Object = UnityEngine.Object;

namespace ZoDream.BundleExtractor
{
    internal partial class UnityBundleChunkReader
    {
        private void ProcessAssets(CancellationToken token)
        {
            var progress = Logger?.CreateSubProgress("Process assets...", _assetItems.Count);
            foreach (var asset in _assetItems)
            {
                for (var i = 0; i < asset.Count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger?.Info("Processing assets has been cancelled !!");
                        return;
                    }
                    if (asset[i] is SpriteAtlas m_SpriteAtlas && m_SpriteAtlas.RenderDataMap.Count > 0)
                    {
                        foreach (var m_PackedSprite in m_SpriteAtlas.PackedSprites)
                        {
                            if (m_PackedSprite.TryGet(out var m_Sprite))
                            {
                                if (!m_Sprite.SpriteAtlas.IsNotNull)
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
                    PrepareExport(i, asset);
                }
                progress?.Add(1);
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
            var progress = Logger?.CreateSubProgress(
                _dependency is not null ? "Build dependencies ... " : "Read assets...",
                _assetItems.Count);
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
                    var info = asset.Get(i);
                    if (_dependency is null && IsExclude((NativeClassID)info.ClassID))
                    {
                        continue;
                    }
                    try
                    {
                        var reader = asset.OpenRead(i);
                        var targetType = ConvertToClassType((NativeClassID)info.ClassID);
                        var doc = asset.GetType(i);
                        object? res = null;
                        // 默认 object 不做转化，所以为 null
                        if (serializer.Converters.TryGet(targetType, out var cvt))
                        {
                            if (doc?.Count > 0 && cvt is IElementTypeLoader tl)
                            {
                                res = tl.Read(reader, targetType, doc);
                            }
                            else
                            {
                                res = cvt.Read(reader, targetType, serializer);
                            }
                        } 
                        else
                        {
                            res = serializer.Deserialize(reader, targetType);
                        }

                        if (res is Object o)
                        {
                            asset[i] = o;
                            _dependency?.AddEntry(asset.FullPath, info.FileID, o.Name, info.ClassID);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger?.Log(LogLevel.Error, e, $"<{info.TypeID}>{info.FileID} of {asset.FullPath}");
                        //if (GC.GetTotalMemory(false) > Math.Pow(1024, 3) * 5)
                        //{
                        //    // 限制最大内存占用
                        //    return;
                        //}
                    }
                }
                progress?.Add(1);
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
                NativeClassID.AssetBundleManifest => typeof(AssetBundleManifest),
                _ => typeof(Object),
            };
        }

        public T? ConvertTo<T>(ISerializedFile asset, int entryId)
        {
            var serializer = _service.Get<IBundleSerializer>();
            var reader = asset.OpenRead(entryId);
            var targetType = typeof(T);//ConvertToClassType((NativeClassID)obj.ClassID);
            var doc = asset.GetType(entryId);
            if (serializer.Converters.TryGet(targetType, out var cvt))
            {
                if (doc?.Count > 0 && cvt is IElementTypeLoader tl)
                {
                    return (T)tl.Read(reader, targetType, doc);
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
