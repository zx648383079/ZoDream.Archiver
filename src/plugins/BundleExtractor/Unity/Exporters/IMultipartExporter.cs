using System;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal interface IMultipartExporter : IBundleExporter, IDisposable
    {
        /// <summary>
        /// 原始路径
        /// </summary>
        public string SourcePath { get; }
        public bool IsEmpty { get; }
    }
}
