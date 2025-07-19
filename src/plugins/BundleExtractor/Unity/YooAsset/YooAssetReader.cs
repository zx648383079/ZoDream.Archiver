using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.YooAsset
{
    public class YooAssetReader(Stream input, string fileName, YooAssetType assetType)
    {
        public const uint Signature = 0x594F4F; // OOY\0
        private readonly PackageManifest _data = Deserialize(input);

        public IEnumerable<KeyValuePair<string, string>> Read()
        {
            var folder = Path.GetDirectoryName(fileName);
            if (assetType == YooAssetType.Hotfix)
            {
                folder = Path.Combine(Path.GetDirectoryName(folder), "CacheBundleFiles");
            }
            foreach (var item in _data.BundleList)
            {
                if (assetType == YooAssetType.Hotfix)
                {
                    var sourcePath = Path.Combine(folder, item.FileHash[..2], item.FileHash, "__data");
                    var outputPath = ConvertPath(item.BundleName);
                    yield return new KeyValuePair<string, string>(sourcePath, outputPath);
                }
                else
                {
                    var sourcePath = Path.Combine(folder, item.FileHash + ".bundle"); // 后缀不确定
                    var outputPath = Path.Combine("Updates", ConvertPath(item.BundleName));
                    yield return new KeyValuePair<string, string>(sourcePath, outputPath);
                }
            }
        }

        public void Write(IBundleMapper mapper)
        {
            foreach (var item in Read())
            {
                mapper.Add(item.Key, item.Value);
            }
        }


        private static string ConvertPath(string bundleName)
        {
            var i = bundleName.LastIndexOf('.');
            if (i >= 0)
            {
                return bundleName[..i].Replace('_', Path.PathSeparator) + bundleName[i..];
            }
            return bundleName.Replace('_', Path.PathSeparator);
        }

        private static PackageManifest Deserialize(Stream input)
        {
            return Deserialize(new BundleBinaryReader(input, Shared.Models.EndianType.LittleEndian));
        }

        private static PackageManifest Deserialize(IBundleBinaryReader reader)
        {
            Expectation.ThrowIfNotSignature(reader.ReadUInt32() == Signature);
            var res = new PackageManifest()
            {
                FileVersion = Version.Parse(ReadString(reader)),// "1.5.2", "2.0.0", "2.3.12"
                EnableAddressable = reader.ReadBoolean(),
                LocationToLower = reader.ReadBoolean(),
                IncludeAssetGUID = reader.ReadBoolean(),
                OutputNameStyle = reader.ReadInt32(),
            };
            if (res.FileVersion.Major >= 2)
            {
                if (res.FileVersion.Minor >= 3) //2.3.12 
                {
                    res.BuildBundleType = reader.ReadInt32();
                }
                res.BuildPipeline = ReadString(reader);
            }
            res.PackageName = ReadString(reader);
            res.PackageVersion = ReadString(reader);
            if (res.FileVersion.Major >= 2 && res.FileVersion.Minor >= 3)
            {
                res.PackageNote = ReadString(reader);
            }
            res.AssetList = reader.ReadArray(() => {
                var asset = new PackageAsset()
                {
                    Address = ReadString(reader),
                    AssetPath = ReadString(reader),
                    AssetGUID = ReadString(reader),
                    AssetTags = reader.ReadArray(reader.ReadUInt16(), ReadString),
                    BundleID = reader.ReadInt32(),
                };
                if (res.FileVersion.Major < 2)
                {
                    asset.DependIDs = reader.ReadArray(reader.ReadUInt16(), r => r.ReadInt32());
                } else if (res.FileVersion.Minor >= 3)
                {
                    asset.DependBundleIDs = reader.ReadArray(reader.ReadUInt16(), r => r.ReadInt32());
                }
                return asset;
            });
            res.BundleList = reader.ReadArray(() => {
                var bundle = new PackageBundle()
                {
                    BundleName = ReadString(reader),
                    UnityCRC = reader.ReadUInt32(),
                    FileHash = ReadString(reader),
                    FileCRC = ReadString(reader),
                    FileSize = reader.ReadInt64(),
                };
                if (res.FileVersion.Major < 2)
                {
                    bundle.IsRawFile = reader.ReadBoolean();
                    bundle.LoadMethod = reader.ReadByte();
                    bundle.Tags = reader.ReadArray(reader.ReadUInt16(), ReadString);
                    bundle.ReferenceIds = reader.ReadArray(reader.ReadUInt16(), r => r.ReadInt32());
                } else
                {
                    bundle.Encrypted = reader.ReadBoolean();
                    bundle.Tags = reader.ReadArray(reader.ReadUInt16(), ReadString);
                    if (res.FileVersion.Minor < 3)
                    {
                        bundle.DependIDs = reader.ReadArray(reader.ReadUInt16(), r => r.ReadInt32());
                    } else
                    {
                        bundle.DependBundleIDs = reader.ReadArray(reader.ReadUInt16(), r => r.ReadInt32());
                    }
                }
                return bundle;
            });
            return res;
        }

        private static string ReadString(IBundleBinaryReader reader)
        {
            return Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadUInt16()));
        }

        
    }

    public enum YooAssetType
    {
        Apk,
        Hotfix
    }
}
