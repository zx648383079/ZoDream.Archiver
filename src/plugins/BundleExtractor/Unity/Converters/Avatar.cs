using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class AvatarConverter : BundleConverter<Avatar>
    {
        public override Avatar? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new Avatar
            {
                Name = reader.ReadAlignedString(),
                AvatarSize = reader.ReadUInt32(),
                Value = serializer.Deserialize<AvatarConstant>(reader)
            };

            int numTOS = reader.ReadInt32();
            res.TOS = [];
            for (int i = 0; i < numTOS; i++)
            {
                res.TOS.Add(reader.ReadUInt32(), reader.ReadAlignedString());
            }

            //HumanDescription m_HumanDescription 2019 and up
            return res;
        }

        public static string FindBonePath(Avatar res, uint hash)
        {
            res.TOS.TryGetValue(hash, out string path);
            return path;
        }
    }
}
