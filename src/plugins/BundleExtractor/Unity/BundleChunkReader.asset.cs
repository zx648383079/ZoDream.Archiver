using System;
using ZoDream.BundleExtractor.Unity.UI;

namespace ZoDream.BundleExtractor
{
    public partial class UnityBundleChunkReader
    {
        private void ProcessAssets()
        {
            foreach (var assetsFile in _assetItems)
            {
                foreach (var obj in assetsFile.Children)
                {
                    //if (tokenSource.IsCancellationRequested)
                    //{
                    //    Logger.Info("Processing assets has been cancelled !!");
                    //    return;
                    //}
                    if (obj is GameObject m_GameObject)
                    {
                        //if (Logger.Flags.HasFlag(LoggerEvent.Verbose))
                        //{
                        //    Logger.Verbose($"GameObject with {m_GameObject.m_PathID} in file {m_GameObject.assetsFile.fileName} has {m_GameObject.m_Components.Count} components, Attempting to fetch them...");
                        //}
                        foreach (var pptr in m_GameObject.m_Components)
                        {
                            if (pptr.TryGet(out var m_Component))
                            {
                                switch (m_Component)
                                {
                                    case Transform m_Transform:
                                        //if (Logger.Flags.HasFlag(LoggerEvent.Verbose))
                                        //{
                                        //    Logger.Verbose($"Fetched Transform component with {m_Transform.m_PathID} in file {m_Transform.assetsFile.fileName}, assigning to GameObject components...");
                                        //}
                                        m_GameObject.m_Transform = m_Transform;
                                        break;
                                    case MeshRenderer m_MeshRenderer:
                                        //if (Logger.Flags.HasFlag(LoggerEvent.Verbose))
                                        //{
                                        //    Logger.Verbose($"Fetched MeshRenderer component with {m_MeshRenderer.m_PathID} in file {m_MeshRenderer.assetsFile.fileName}, assigning to GameObject components...");
                                        //}
                                        m_GameObject.m_MeshRenderer = m_MeshRenderer;
                                        break;
                                    case MeshFilter m_MeshFilter:
                                        //if (Logger.Flags.HasFlag(LoggerEvent.Verbose))
                                        //{
                                        //    Logger.Verbose($"Fetched MeshFilter component with {m_MeshFilter.m_PathID} in file {m_MeshFilter.assetsFile.fileName}, assigning to GameObject components...");
                                        //}
                                        m_GameObject.m_MeshFilter = m_MeshFilter;
                                        break;
                                    case SkinnedMeshRenderer m_SkinnedMeshRenderer:
                                        //if (Logger.Flags.HasFlag(LoggerEvent.Verbose))
                                        //{
                                        //    Logger.Verbose($"Fetched SkinnedMeshRenderer component with {m_SkinnedMeshRenderer.m_PathID} in file {m_SkinnedMeshRenderer.assetsFile.fileName}, assigning to GameObject components...");
                                        //}
                                        m_GameObject.m_SkinnedMeshRenderer = m_SkinnedMeshRenderer;
                                        break;
                                    case Animator m_Animator:
                                        //if (Logger.Flags.HasFlag(LoggerEvent.Verbose))
                                        //{
                                        //    Logger.Verbose($"Fetched Animator component with {m_Animator.m_PathID} in file {m_Animator.assetsFile.fileName}, assigning to GameObject components...");
                                        //}
                                        m_GameObject.m_Animator = m_Animator;
                                        break;
                                    case Animation m_Animation:
                                        //if (Logger.Flags.HasFlag(LoggerEvent.Verbose))
                                        //{
                                        //    Logger.Verbose($"Fetched Animation component with {m_Animation.m_PathID} in file {m_Animation.assetsFile.fileName}, assigning to GameObject components...");
                                        //}
                                        m_GameObject.m_Animation = m_Animation;
                                        break;
                                }
                            }
                        }
                    }
                    else if (obj is SpriteAtlas m_SpriteAtlas)
                    {
                        if (m_SpriteAtlas.m_RenderDataMap.Count > 0)
                        {
                            //if (Logger.Flags.HasFlag(LoggerEvent.Verbose))
                            //{
                            //    Logger.Verbose($"SpriteAtlas with {m_SpriteAtlas.m_PathID} in file {m_SpriteAtlas.assetsFile.fileName} has {m_SpriteAtlas.m_PackedSprites.Count} packed sprites, Attempting to fetch them...");
                            //}
                            foreach (var m_PackedSprite in m_SpriteAtlas.m_PackedSprites)
                            {
                                if (m_PackedSprite.TryGet(out var m_Sprite))
                                {
                                    if (m_Sprite.m_SpriteAtlas.IsNull)
                                    {
                                        //if (Logger.Flags.HasFlag(LoggerEvent.Verbose))
                                        //{
                                        //    Logger.Verbose($"Fetched Sprite with {m_Sprite.m_PathID} in file {m_Sprite.assetsFile.fileName}, assigning to parent SpriteAtlas...");
                                        //}
                                        m_Sprite.m_SpriteAtlas.Set(m_SpriteAtlas);
                                    }
                                    else
                                    {
                                        m_Sprite.m_SpriteAtlas.TryGet(out var m_SpriteAtlaOld);
                                        if (m_SpriteAtlaOld.m_IsVariant)
                                        {
                                            //if (Logger.Flags.HasFlag(LoggerEvent.Verbose))
                                            //{
                                            //    Logger.Verbose($"Fetched Sprite with {m_Sprite.m_PathID} in file {m_Sprite.assetsFile.fileName} has a variant of the origianl SpriteAtlas, disposing of the variant and assinging to the parent SpriteAtlas...");
                                            //}
                                            m_Sprite.m_SpriteAtlas.Set(m_SpriteAtlas);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ReadAssets()
        {
            foreach (var asset in _assetItems)
            {
                foreach (var obj in asset.ObjectMetaItems)
                {
                    var reader = new UIReader(asset.Create(obj), obj, asset, _options);
                    try
                    {
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
                            ElementIDType.MonoBehaviour => new MonoBehaviour(reader),
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
                            ElementIDType.Transform => new Transform(reader),
                            ElementIDType.VideoClip => new VideoClip(reader),
                            ElementIDType.ResourceManager => new ResourceManager(reader),
                            _ => new UIObject(reader),
                        };
                        asset.AddChild(res);
                    }
                    catch (Exception e)
                    {
                        //var sb = new StringBuilder();
                        //sb.AppendLine("Unable to load object")
                        //    .AppendLine($"Assets {assetsFile.fileName}")
                        //    .AppendLine($"Path {assetsFile.originalPath}")
                        //    .AppendLine($"Type {objectReader.type}")
                        //    .AppendLine($"PathID {objectInfo.m_PathID}")
                        //    .Append(e);
                        //Logger.Error(sb.ToString());
                    }
                }
            }
        }

    }
}
