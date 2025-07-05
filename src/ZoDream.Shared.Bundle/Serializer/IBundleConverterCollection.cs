using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleConverterCollection : ICollection<IBundleConverter>
    {
        public bool TryGet<T>([NotNullWhen(true)] out IBundleConverter? converter);
        public bool TryGet(Type objectType, [NotNullWhen(true)] out IBundleConverter? converter);
    }
}