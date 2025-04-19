using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Bundle
{
    public class BundleConverterCollection : Collection<IBundleConverter>, IBundleConverterCollection
    {
        public bool TryGet<T>([NotNullWhen(true)] out IBundleConverter? converter)
        {
            return TryGet(typeof(T), out converter);
        }

        public bool TryGet(Type objectType, [NotNullWhen(true)] out IBundleConverter? converter)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].CanConvert(objectType))
                {
                    converter = this[i];
                    return true;
                }
            }
            converter = null;
            return false;
        }
    }
}
