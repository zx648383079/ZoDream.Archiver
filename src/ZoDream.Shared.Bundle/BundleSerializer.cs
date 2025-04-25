using System;
using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public class BundleSerializer : IBundleSerializer
    {
        public BundleSerializer()
        {
            Converters = new BundleConverterCollection();
        }
        public BundleSerializer(IEnumerable<IBundleConverter> items)
        {
            Converters = items is IBundleConverterCollection o ? o : new BundleConverterCollection([.. items]); 
        }
        public IBundleConverterCollection Converters { get; private set; }

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
