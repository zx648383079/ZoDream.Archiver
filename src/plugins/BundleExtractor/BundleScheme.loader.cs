using System;
using System.Linq;
using ZoDream.BundleExtractor.Engines;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        private readonly Type[] _loaderItems = [
            typeof(Engines.UnityEngine),
            typeof(CocosEngine),
            typeof(UnrealEngine),
            typeof(GodotEngine),
            typeof(KrKrEngine),
            typeof(RenPyEngine),
            typeof(RPGMakerEngine),
            typeof(TyranoEngine),
            typeof(XnaEngine),
            typeof(RSGameEngine),
            typeof(UnknownEngine),

            typeof(WindowsPlatformScheme),
            typeof(AndroidPlatformScheme),
            typeof(IosPlatformScheme),
            typeof(LinuxPlatformScheme),
            typeof(MacPlatformScheme),
            typeof(PlayStationPlatformScheme),
            typeof(SwitchPlatformScheme),
            typeof(WebGLPlatformScheme),
            typeof(WiiUPlatformScheme),
            typeof(UnknownPlatform),

            typeof(MiHoYoProducer),
            typeof(PaperProducer),
            typeof(UnknownProducer),
        ];

        private void Initialize()
        {
            var items = _loaderItems.Select(Service.Get);
            _engineItems = items.Where(x => x is IBundleEngine).Select(x => (IBundleEngine)x).ToArray();
            _platformItems = items.Where(x => x is IBundlePlatform).Select(x => (IBundlePlatform)x).ToArray();
            _producerItems = items.Where(x => x is IBundleProducer).Select(x => (IBundleProducer)x).ToArray();
        }

        public T Get<T>(IBundleOptions options)
            where T : IBundleLoader
        {
            var type = typeof(T);
            return (T)Get(GetLoaderName(type, options), GetLoaders(type));
        }

        public bool TryGet<T>(IBundleSource fileItems, IBundleOptions options)
            where T : IBundleLoader
        {
            var items = GetLoaders(typeof(T));
            foreach (var item in items)
            {
                if (item.TryLoad(fileItems, options))
                {
                    return true;
                }
            }
            return false;
        }

        private string GetLoaderName(Type type, IBundleOptions options)
        {
            if (type == typeof(IBundleEngine))
            {
                return options.Engine ?? string.Empty;
            }
            if (type == typeof(IBundlePlatform))
            {
                return options.Platform ?? string.Empty;
            }
            if (type == typeof(IBundleProducer))
            {
                return options.Producer ?? string.Empty;
            }
            return string.Empty;
        }

        private IBundleLoader[] GetLoaders(Type type)
        {
            if (type == typeof(IBundleEngine))
            {
                return _engineItems;
            }
            if (type == typeof(IBundlePlatform))
            {
                return _platformItems;
            }
            if (type == typeof(IBundleProducer))
            {
                return _producerItems;
            }
            return [];
        }

        private static T Get<T>(string? name, T[] items)
            where T : IBundleLoader
        {
            T def = default;
            foreach (var item in items)
            {
                if (item.AliasName == name)
                {
                    return item;
                }
                if (string.IsNullOrWhiteSpace(item.AliasName))
                {
                    def = item;
                }
            }
            return def;
        }

        private static string[] GetNames<T>(T[] items)
            where T : IBundleLoader
        {
            return items.Select(i => i.AliasName).Where(i => !string.IsNullOrWhiteSpace(i)).ToArray();
        }
    }
}
