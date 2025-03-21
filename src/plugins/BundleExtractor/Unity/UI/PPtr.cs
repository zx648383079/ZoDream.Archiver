using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class PPtr<T> where T : UIObject
    {
        public int m_FileID;
        public long m_PathID;

        private readonly ISerializedFile assetsFile;
        private int index = -2; //-2 - Prepare, -1 - Missing

        public string Name => TryGet(out var obj) ? obj.Name : string.Empty;

        public PPtr(int m_FileID, long m_PathID, ISerializedFile assetsFile)
        {
            this.m_FileID = m_FileID;
            this.m_PathID = m_PathID;
            this.assetsFile = assetsFile;
        }

        public PPtr(IBundleBinaryReader reader)
        {
            m_FileID = reader.ReadInt32();
            m_PathID = reader.Get<FormatVersion>() < FormatVersion.Unknown_14 ?
                reader.ReadInt32() : reader.ReadInt64();
            assetsFile = reader.Get<ISerializedFile>();
        }

        //public YAMLNode ExportYAML(int[] version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Style = MappingStyle.Flow;
        //    node.Add("fileID", m_FileID);
        //    return node;
        //}

        private bool TryGetAssetsFile([NotNullWhen(true)] out ISerializedFile? result)
        {
            result = null;
            if (m_FileID == 0)
            {
                result = assetsFile;
                return true;
            }

            if (m_FileID > 0 && m_FileID - 1 < assetsFile.Dependencies.Count())
            {
                var assetsManager = assetsFile.Container;
                if (assetsManager is null)
                {
                    return false;
                }
                if (index == -2)
                {
                    var name = assetsFile.GetDependency(m_FileID - 1);
                    index = assetsManager.IndexOf(name);
                }

                if (index >= 0)
                {
                    result = assetsManager[index];
                    return true;
                }
            }

            return false;
        }

        public bool TryGet([NotNullWhen(true)] out T? result)
        {
            if (TryGetAssetsFile(out var sourceFile))
            {
                var obj = sourceFile?[m_PathID];
                if (obj is not null)
                {
                    if (obj is T variable)
                    {
                        result = variable;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public bool TryGet<T2>([NotNullWhen(true)] out T2? result) where T2 : UIObject
        {
            if (TryGetAssetsFile(out var sourceFile))
            {
                var obj = sourceFile?[m_PathID];
                if (obj is not null)
                {
                    if (obj is T2 variable)
                    {
                        result = variable;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public void Set(T m_Object)
        {
            var name = m_Object.AssetFile.FullPath;
            if (string.Equals(assetsFile.FullPath, name, StringComparison.OrdinalIgnoreCase))
            {
                m_FileID = 0;
            }
            else
            {
                m_FileID = assetsFile.IndexOf(name);
                if (m_FileID == -1)
                {
                    m_FileID = assetsFile.AddDependency(m_Object.AssetFile.FullPath);
                }
                m_FileID += 1;
            }

            var assetsManager = assetsFile.Container;


            index = assetsManager.IndexOf(name);

            m_PathID = m_Object.FileID;
        }

        public PPtr<T2> Cast<T2>() where T2 : UIObject
        {
            return new PPtr<T2>(m_FileID, m_PathID, assetsFile);
        }

        public bool IsNull => m_PathID == 0 || m_FileID < 0;
    }
}
