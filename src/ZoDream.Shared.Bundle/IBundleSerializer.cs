using System;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleSerializer
    {
        public IBundleConverterCollection Converters { get; }

        public T? Deserialize<T>(IBundleBinaryReader reader);
        public object? Deserialize(IBundleBinaryReader reader, Type objectType);

    }
}