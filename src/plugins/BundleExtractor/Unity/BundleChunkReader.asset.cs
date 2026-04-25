using System;
using System.Threading;
using UnityEngine;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;
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
                if (!IsEntry(asset))
                {
                    progress?.Add(1);
                    continue;
                }
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

        private bool IsEntry(ISerializedFile asset)
        {
            return IsEntry(asset.FullPath);
        }

        private bool IsEntry(IFilePath filePath)
        {
            return _entryItems.Contains(FilePath.GetFilePath(filePath));
        }

        private void ReadAssets(CancellationToken token)
        {
            var progress = Logger?.CreateSubProgress(
                _dependency is not null ? "Build dependencies ... " : "Read assets...",
                _assetItems.Count);
            var serializer = _service.Get<IBundleSerializer>();
            foreach (var asset in _assetItems)
            {
                if (!IsEntry(asset))
                {
                    progress?.Add(1);
                    continue;
                }
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
                    ReadAsset(asset, i, info, serializer);
                }
                progress?.Add(1);
            }
        }

        public Object? ReadAsset(ISerializedFile asset, int entryId)
        {
            return ReadAsset(asset, entryId, asset.Get(entryId), _service.Get<IBundleSerializer>());
        }

        private Object? ReadAsset(ISerializedFile asset, int entryId, 
            ObjectInfo info, IBundleSerializer serializer)
        {
            try
            {
                var reader = asset.OpenRead(entryId);
                var targetType = ConvertToClassType((NativeClassID)info.ClassID);
                var doc = asset.GetType(entryId);
                object? res = null;
                // 默认 object 不做转化，所以为 null
                if (doc?.Count > 0 && serializer.Converters.TryGet(targetType, out var cvt)
                    && cvt is ITypeTreeConverter tl)
                {
                    res = tl.Read(reader, targetType, doc);
                }
                else
                {
                    res = serializer.Deserialize(reader, targetType);
                }

                if (res is Object o)
                {
                    asset[entryId] = o;
                    _dependency?.AddEntry(asset.FullPath, info.FileID, o.Name, info.ClassID);
                    return o;
                }
                asset[entryId] = LoadedFailureObject.Instance;
            }
            catch (Exception e)
            {
                asset[entryId] = LoadedFailureObject.Instance;
                Logger?.Log(LogLevel.Error, e, $"<{info.TypeID}>{info.FileID} of {asset.FullPath}");
                //if (GC.GetTotalMemory(false) > Math.Pow(1024, 3) * 5)
                //{
                //    // 限制最大内存占用
                //    return;
                //}
            }
            return null;
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
                NativeClassID.CanvasRenderer => typeof(CanvasRenderer),
                NativeClassID.RenderTexture => typeof(RenderTexture),
                _ => typeof(Object),
            };
        }

        public T? Get<T>(ISerializedFile asset, int entryId)
        {
            var exsit = asset[entryId];
            if (exsit is T o)
            {
                return o;
            }
            if (exsit is not null)
            {
                return ConvertTo<T>(asset, entryId);
            }
            if (IsEntry(asset))
            {
                return default;
            }
            var info = asset.Get(entryId);
            var toType = typeof(T);
            var targetType = ConvertToClassType((NativeClassID)info.ClassID);
            if (targetType == toType || targetType.IsAssignableTo(toType))
            {
                return (T)(object)ReadAsset(asset, entryId, info, _service.Get<IBundleSerializer>());
            }
            return ConvertTo<T>(asset, entryId);
        }

        public T? ConvertTo<T>(ISerializedFile asset, int entryId)
        {
            var serializer = _service.Get<IBundleSerializer>();
            var reader = asset.OpenRead(entryId);
            var targetType = typeof(T);//ConvertToClassType((NativeClassID)obj.ClassID);
            var doc = asset.GetType(entryId);
            if (doc?.Count > 0 && serializer.Converters.TryGet(targetType, out var cvt)
                            && cvt is ITypeTreeConverter tl)
            {
                return (T)tl.Read(reader, targetType, doc);
            }
            else
            {
                return (T)serializer.Deserialize(reader, targetType);
            }
        }
    }
}
