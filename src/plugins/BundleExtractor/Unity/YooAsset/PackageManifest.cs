using System;

namespace ZoDream.BundleExtractor.Unity.YooAsset
{
    /// <summary>
    /// 资源包清单
    /// </summary>
    public class PackageManifest
    {
        /// <summary>
        /// 文件版本
        /// </summary>
        public Version FileVersion { get; set; }

        /// <summary>
        /// 启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable { get; set; }

        /// <summary>
        /// 资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower { get; set; }

        /// <summary>
        /// 包含资源GUID数据
        /// </summary>
        public bool IncludeAssetGUID { get; set; }

        /// <summary>
        /// 文件名称样式
        /// </summary>
        public int OutputNameStyle { get; set; }

        /// <summary>
        /// 构建资源包类型
        /// 
        /// 2.3.12版本新增
        /// </summary>
        public int BuildBundleType { get; set; }

        /// <summary>
        /// 构建管线名称
        /// 2.0.0+版本使用
        /// </summary>
        public string BuildPipeline { get; set; }

        /// <summary>
        /// 资源包裹名称
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// 资源包裹的版本信息
        /// </summary>
        public string PackageVersion { get; set; }

        /// <summary>
        /// 资源包裹的备注信息
        /// 2.3.12版本新增
        /// </summary>
        public string PackageNote { get; set; }

        /// <summary>
        /// 资源列表（主动收集的资源列表）
        /// </summary>
        public PackageAsset[] AssetList { get; set; } = [];

        /// <summary>
        /// 资源包列表
        /// </summary>
        public PackageBundle[] BundleList { get; set; } = [];

        public override string ToString()
        {
            return $"[{FileVersion}]{PackageName} {PackageVersion}";
        }
    }

    /// <summary>
    /// 资源包信息
    /// </summary>
    public class PackageBundle
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName { get; set; }

        /// <summary>
        /// Unity引擎生成的CRC
        /// </summary>
        public uint UnityCRC { get; set; }

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string FileHash { get; set; }

        /// <summary>
        /// 文件校验码
        /// </summary>
        public string FileCRC { get; set; }

        /// <summary>
        /// 文件大小（字节数）
        /// </summary>
        public long FileSize { get; set; }

        #region 1.5.2版本字段
        public bool IsRawFile { get; set; }

        public int LoadMethod { get; set; }

        public int[] ReferenceIds { get; set; }
        #endregion

        /// <summary>
        /// 文件是否加密
        /// 2.0.0版本字段
        /// </summary>
        public bool Encrypted { get; set; }
        /// <summary>
        /// 2.0.0版本字段
        /// </summary>
        public int[] DependIDs { get; set; }

        /// <summary>
        /// 资源包的分类标签
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// 依赖的资源包ID集合
        /// 注意：引擎层构建查询结果
        /// 2.3.12版本字段
        /// </summary>
        public int[] DependBundleIDs { get; set; }

        public override string ToString()
        {
            return FileHash;
        }
    }

    public class PackageAsset
    {
        /// <summary>
        /// 可寻址地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; set; }

        /// <summary>
        /// 资源GUID
        /// </summary>
        public string AssetGUID { get; set; }

        /// <summary>
        /// 资源的分类标签
        /// </summary>
        public string[] AssetTags { get; set; }

        /// <summary>
        /// 所属资源包ID
        /// </summary>
        public int BundleID { get; set; }

        /// <summary>
        /// 依赖的资源包ID集合
        /// 说明：框架层收集查询结果
        /// 2.3.12版本中使用
        /// </summary>
        public int[] DependBundleIDs { get; set; }
        /// <summary>
        /// 仅在1.5.2版本中使用
        /// </summary>
        public int[] DependIDs { get; set; }

        public override string ToString()
        {
            return AssetPath;
        }
    }
}
