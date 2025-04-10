﻿using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity
{
    internal interface IBundleContainer
    {
        public IBundleExtractOptions Options { get; }
        /// <summary>
        /// 添加一个不需要导出
        /// </summary>
        /// <param name="fileId"></param>
        public void TryAddExclude(long fileId);
        /// <summary>
        /// 判断一个对象不需要导出
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public bool IsExclude(long fileId);

        /// <summary>
        /// 判断文件在 Asset 中的位置
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public int IndexOf(string fileName);

        public ISerializedFile? this[int index] { get; }
        /// <summary>
        /// 获取资源文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public Stream OpenResource(string fileName, ISerializedFile source);
    }
}
