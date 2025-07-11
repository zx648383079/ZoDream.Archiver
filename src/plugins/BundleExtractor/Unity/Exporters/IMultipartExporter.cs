using System;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal interface IMultipartExporter : IBundleExporter, IDisposable
    {
        /// <summary>
        /// 原始路径
        /// </summary>
        public IFilePath SourcePath { get; }
        public bool IsEmpty { get; }
    }

    internal interface IMultipartBuilder : IMultipartExporter
    {
        /// <summary>
        /// 追加
        /// </summary>
        /// <param name="entryId"></param>
        public void Append(int entryId);
    }
}
