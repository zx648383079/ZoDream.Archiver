using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    internal partial class UnityBundleChunkReader
    {
        private void ProcessAssets(CancellationToken token)
        {
            
            foreach (var assetsFile in _assetItems)
            {
                foreach (var obj in assetsFile.Children)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger.Info("Processing assets has been cancelled !!");
                        return;
                    }
                    if (obj is GameObject m_GameObject)
                    {
                        foreach (var pPtr in m_GameObject.m_Components)
                        {
                            if (pPtr.TryGet(out var m_Component))
                            {
                                switch (m_Component)
                                {
                                    case Transform m_Transform:
                                        m_GameObject.m_Transform = m_Transform;
                                        TryAddExclude(pPtr.m_PathID);
                                        break;
                                    case MeshRenderer m_MeshRenderer:
                                        m_GameObject.m_MeshRenderer = m_MeshRenderer;
                                        TryAddExclude(pPtr.m_PathID);
                                        break;
                                    case MeshFilter m_MeshFilter:
                                        m_GameObject.m_MeshFilter = m_MeshFilter;
                                        TryAddExclude(pPtr.m_PathID);
                                        break;
                                    case SkinnedMeshRenderer m_SkinnedMeshRenderer:
                                        m_GameObject.m_SkinnedMeshRenderer = m_SkinnedMeshRenderer;
                                        TryAddExclude(pPtr.m_PathID);
                                        break;
                                    case Animator m_Animator:
                                        m_GameObject.m_Animator = m_Animator;
                                        TryAddExclude(pPtr.m_PathID);
                                        break;
                                    case Animation m_Animation:
                                        m_GameObject.m_Animation = m_Animation;
                                        TryAddExclude(pPtr.m_PathID);
                                        break;
                                }
                            }
                        }
                    }
                    else if (obj is SpriteAtlas m_SpriteAtlas && m_SpriteAtlas.m_RenderDataMap.Count > 0)
                    {
                        foreach (var m_PackedSprite in m_SpriteAtlas.m_PackedSprites)
                        {
                            if (m_PackedSprite.TryGet(out var m_Sprite))
                            {
                                if (m_Sprite.m_SpriteAtlas.IsNull)
                                {
                                    m_Sprite.m_SpriteAtlas.Set(m_SpriteAtlas);
                                }
                                else if (m_Sprite.m_SpriteAtlas.TryGet(out var m_SpriteAtlasOld) && m_SpriteAtlasOld.m_IsVariant)
                                {
                                    m_Sprite.m_SpriteAtlas.Set(m_SpriteAtlas);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsExclude(ElementIDType type)
        {
            return type switch
            {
                ElementIDType.GameObject or ElementIDType.Mesh or ElementIDType.AnimationClip or ElementIDType.Animator => Options?.EnabledModel != true,
                ElementIDType.Shader => Options?.EnabledShader != true,
                ElementIDType.Texture2D or ElementIDType.Texture or ElementIDType.Sprite or ElementIDType.SpriteAtlas => Options?.EnabledImage != true,
                ElementIDType.AudioClip => Options?.EnabledAudio != true,
                ElementIDType.VideoClip => Options?.EnabledVideo != true,
                ElementIDType.TextAsset => Options?.EnabledLua != true && Options?.EnabledSpine != true && Options?.EnabledJson != true,
                _ => false,
            };
        }

        private void ReadAssets(CancellationToken token)
        {
            var scanner = _service.Get<IBundleElementScanner>();
            
            foreach (var asset in _assetItems)
            {
                foreach (var obj in asset.ObjectMetaItems)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger.Info("Reading assets has been cancelled !!");
                        return;
                    }
                    if (_dependency is null && IsExclude((ElementIDType)obj.ClassID))
                    {
                        continue;
                    }
                    try
                    {
                        _dependency?.AddEntry(asset.FullPath, obj.FileID);
                        var reader = new UIReader(asset.Create(obj), obj, asset, _options);
                        reader.Add(scanner);
                        UIObject res = reader.Type switch
                        {
                            ElementIDType.Animation => new Animation(reader),
                            ElementIDType.AnimationClip => new AnimationClip(reader),
                            ElementIDType.Animator => new Animator(reader),
                            ElementIDType.AnimatorController => new AnimatorController(reader),
                            ElementIDType.AnimatorOverrideController => new AnimatorOverrideController(reader),
                            ElementIDType.AssetBundle => new AssetBundle(reader),
                            ElementIDType.AudioClip => new AudioClip(reader),
                            ElementIDType.Avatar => new Avatar(reader),
                            ElementIDType.Font => new Font(reader),
                            ElementIDType.GameObject => new GameObject(reader),
                            ElementIDType.IndexObject => new IndexObject(reader),
                            ElementIDType.Material => new Material(reader),
                            ElementIDType.Mesh => new Mesh(reader),
                            ElementIDType.MeshFilter => new MeshFilter(reader),
                            ElementIDType.MeshRenderer => new MeshRenderer(reader),
                            ElementIDType.MiHoYoBinData => new MiHoYoBinData(reader),
                            ElementIDType.MonoBehavior => new MonoBehavior(reader),
                            ElementIDType.MonoScript => new MonoScript(reader),
                            ElementIDType.MovieTexture => new MovieTexture(reader),
                            ElementIDType.PlayerSettings => new PlayerSettings(reader),
                            ElementIDType.RectTransform => new RectTransform(reader),
                            ElementIDType.Shader => new Shader(reader),
                            ElementIDType.SkinnedMeshRenderer => new SkinnedMeshRenderer(reader),
                            ElementIDType.Sprite => new Sprite(reader),
                            ElementIDType.SpriteAtlas => new SpriteAtlas(reader),
                            ElementIDType.TextAsset => new TextAsset(reader),
                            ElementIDType.Texture2D => new Texture2D(reader),
                            ElementIDType.Texture2DArray => new Texture2D(reader),
                            ElementIDType.Transform => new Transform(reader),
                            ElementIDType.VideoClip => new VideoClip(reader),
                            ElementIDType.ResourceManager => new ResourceManager(reader),
                            _ => new UIObject(reader),
                        };
                        if (reader.SerializedType.OldType is not null && 
                            reader.SerializedType.OldType.Nodes.Count > 0 && 
                            res is IElementTypeLoader tl)
                        {
                            tl.Read(reader, reader.SerializedType.OldType);
                        } else if (
                            res is IElementLoader l
                            && scanner.TryRead(reader, l))
                        {}
                        asset.AddChild(res);
                        if (_dependency is not null) 
                        {
                            res.Associated(_dependency);
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
                        Logger.Error(e.Message);
                    }
                }
            }
        }

    }
}
