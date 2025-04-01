using System;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public partial class BundleOptions: ArchiveOptions, IBundleOptions
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
        /// 应用显示名称
        /// </summary>
        public string? DisplayName { get; set; }
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


        public BundleOptions()
        {
            
        }

        public BundleOptions(string password): base(password)
        {
        }

        public BundleOptions(IArchiveOptions? options): base(options)
        {
            if (options is not IBundleOptions o)
            {
                return;
            }
            Engine = o.Engine;
            Platform = o.Platform;
            Package = o.Package;
            Producer = o.Producer;
            Version = o.Version;
            Entrance = o.Entrance;
            DisplayName = o.DisplayName;
        }

        public void Load(IBundleOptions? options)
        {
            if (options is null)
            {
                return;
            }
            Engine = options.Engine;
            Platform = options.Platform;
            Package = options.Package;
            Producer = options.Producer;
            Version = options.Version;
            Entrance = options.Entrance;
            DisplayName = options.DisplayName;
        }
    }
}
