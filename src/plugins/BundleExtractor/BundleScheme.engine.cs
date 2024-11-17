using System;
using System.Linq;
using ZoDream.BundleExtractor.Engines;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        private readonly IBundleEngine[] _engineItems = [
            new UnityEngine(),
            new CocosEngine(),
            new UnrealEngine(),
            new GodotEngine(),
            new KrKrEngine(),
            new RenPyEngine(),
            new RPGMakerEngine(),
            new TyranoEngine(),
            new UnknownEngine(),
        ];

        public string[] EngineNames => GetNames(_engineItems);

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
