using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class PPtrConverter : BundleConverter<PPtr>
    {
        private int index = -2; //-2 - Prepare, -1 - Missing


        public override PPtr? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = Activator.CreateInstance(objectType) as PPtr;
            res.FileID = reader.ReadInt32();
            res.PathID = reader.Get<FormatVersion>() < FormatVersion.Unknown_14 ?
                reader.ReadInt32() : reader.ReadInt64();
            return res;
        }


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
                    result = assetsFile.Container?.ConvertTo<T>(obj.Reader);
                    return result is not null;
                }
            }
            if (!IsNull)
            {
                assetsFile.Logger?.Warning($"Need: [{m_FileID}]{m_PathID}");
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
                    result = assetsFile.Container?.ConvertTo<T2>(obj.Reader);
                    return result is not null;
                }
            }
            if (!IsNull)
            {
                assetsFile.Logger?.Warning($"Need: [{m_FileID}]{m_PathID}");
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
