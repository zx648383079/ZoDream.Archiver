using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleOptions : IArchiveOptions
    {
        /// <summary>
        /// 开发工具/游戏引擎
        /// </summary>
        public string? Engine { get; set; }
        /// <summary>
        /// 使用环境平台
        /// </summary>
        public string? Platform { get; set; }
        /// <summary>
        /// 包名
        /// </summary>
        public string? Package { get; set; }
        /// <summary>
        /// 制作人
        /// </summary>
        public string? Producer { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string? Version { get; set; }
        /// <summary>
        /// 入口根目录
        /// </summary>
        public string? Entrance { get; set; }

    }
}
