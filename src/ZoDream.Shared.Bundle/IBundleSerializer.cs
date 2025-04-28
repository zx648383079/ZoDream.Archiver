using System;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleSerializer
    {
        /// <summary>
        /// 默认倒叙查找，所以 子类转换器请放在父类转换器之后
        /// </summary>
        public IBundleConverterCollection Converters { get; }

        public T? Deserialize<T>(IBundleBinaryReader reader);
        public object? Deserialize(IBundleBinaryReader reader, Type objectType);

    }
}