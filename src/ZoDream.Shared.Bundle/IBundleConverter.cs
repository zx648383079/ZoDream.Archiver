using System;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleConverter
    {

        public bool CanConvert(Type objectType);

        public object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer);
    }

    public interface IBundleConverter<T>
    {
        public T? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer);
    }

    public interface IBundlePipelineConverter<T>
    {
        public void Read(ref T instance, IBundleBinaryReader reader, IBundleSerializer serializer);
    }
}
