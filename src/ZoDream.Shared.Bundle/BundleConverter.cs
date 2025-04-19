using System;

namespace ZoDream.Shared.Bundle
{
    public abstract class BundleConverter : IBundleConverter
    {
        public abstract bool CanConvert(Type objectType);

        public abstract object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer);
    }

    public abstract class BundleConverter<T> : IBundleConverter<T>
    {
        public bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public abstract T? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer);
    }
}
