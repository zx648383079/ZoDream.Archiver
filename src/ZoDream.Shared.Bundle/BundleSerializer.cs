using System;

namespace ZoDream.Shared.Bundle
{
    public class BundleSerializer : IBundleSerializer
    {
        public IBundleConverterCollection Converters { get; private set; } = new BundleConverterCollection();

        public T? Deserialize<T>(IBundleBinaryReader reader)
        {
            return (T?)Deserialize(reader, typeof(T));
        }

        public object? Deserialize(IBundleBinaryReader reader, Type objectType)
        {
            if (Converters.TryGet(objectType, out var converter))
            {
                return converter.Read(reader, objectType, this);
            }
            return null;
        }
    }
}
