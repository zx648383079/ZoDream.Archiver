using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public interface IDependencyEntry
    {
        public string Path { get; }
        /// <summary>
        /// 定义在依赖文件中的
        /// </summary>
        public long Offset { get; }

        public IList<string> Dependencies { get; }

        public void Add(params string[] dependencies);
    }

    public interface IDependencyDictionary : IDictionary<string, IDependencyEntry>
    {

        public string Entrance { get; }
        /// <summary>
        /// 添加依赖
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dependencies"></param>
        public void Add(string key, params string[] dependencies);
        /// <summary>
        /// 根据文件路径获取文件所需依赖路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public bool TryGet(string fileName, out string[] items);
        public bool TryGet(IEnumerable<string> files, out string[] items);
    }
}
