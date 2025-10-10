using System;
using UnityEngine.Document;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    /// <summary>
    /// 可以根据类型字典库读取
    /// </summary>
    internal interface ITypeTreeConverter
    {

        public object? Read(IBundleBinaryReader reader, Type target, VirtualDocument typeMaps);
    }
}
