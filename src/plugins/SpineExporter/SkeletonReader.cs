using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.SpineExporter.Models;

namespace ZoDream.SpineExporter
{
    /// <summary>
    /// skel 文件读取
    /// </summary>
    public partial class SkeletonReader
    {
        public Task<IEnumerable<SkeletonRoot>?> ReadAsync(string fileName)
        {
            return Task.Factory.StartNew(() => {
                using var fs = File.OpenRead(fileName);
                return Read(fs);
            });
        }

        public Task WriteAsync(string fileName, IEnumerable<SkeletonRoot> data)
        {
            throw new NotImplementedException();
        }

    }
}
