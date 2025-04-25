using System;
using System.Linq;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ShaderConverter : BundleConverter<Shader>
    {
        public static void ReadBase(Shader res, IBundleBinaryReader reader, 
            IBundleSerializer serializer, Action cb)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            res.Name = reader.ReadAlignedString();
            if (version.GreaterThanOrEquals(5, 5)) //5.5 and up
            {
                res.ParsedForm = serializer.Deserialize<SerializedShader>(reader);
                res.Platforms = reader.ReadArray(r => r.ReadUInt32()).Select(x => (ShaderCompilerPlatform)x).ToArray();
                if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
                {
                    res.Offsets = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                    res.CompressedLengths = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                    res.DecompressedLengths = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                }
                else
                {
                    res.Offsets = reader.ReadArray(r => r.ReadUInt32()).Select(x => new[] { x }).ToArray();
                    res.CompressedLengths = reader.ReadArray(r => r.ReadUInt32()).Select(x => new[] { x }).ToArray();
                    res.DecompressedLengths = reader.ReadArray(r => r.ReadUInt32()).Select(x => new[] { x }).ToArray();
                }
                res.CompressedBlob = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
                cb.Invoke();

                if (version.GreaterThanOrEquals(2021, 3, 12, VersionType.Final, 1) || //2021.3.12f1 and up
                    version.GreaterThanOrEquals(2022, 1, 21, VersionType.Final, 1)) //2022.1.21f1 and up
                {
                    res.StageCounts = reader.ReadArray(r => r.ReadUInt32());
                }

                var m_DependenciesCount = reader.ReadInt32();
                for (int i = 0; i < m_DependenciesCount; i++)
                {
                    _ = reader.ReadPPtr<Shader>(serializer);
                }

                if (version.GreaterThanOrEquals(2018))
                {
                    var m_NonModifiableTexturesCount = reader.ReadInt32();
                    for (int i = 0; i < m_NonModifiableTexturesCount; i++)
                    {
                        var first = reader.ReadAlignedString();
                        reader.ReadPPtr<Texture>(serializer);
                    }
                }

                var m_ShaderIsBaked = reader.ReadBoolean();
                reader.AlignStream();
            }
            else
            {
                res.Script = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
                var m_PathName = reader.ReadAlignedString();
                if (version.Major == 5 && version.Minor >= 3) //5.3 - 5.4
                {
                    res.DecompressedSize = reader.ReadUInt32();
                    res.SubProgramBlob = reader.ReadArray(r => r.ReadByte());
                }
            }
        }

        public override Shader? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Shader();
            ReadBase(res, reader, serializer, () => {
            });
            return res;
        }

    }
}
