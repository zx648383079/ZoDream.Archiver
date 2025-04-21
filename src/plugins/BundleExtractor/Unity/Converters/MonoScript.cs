using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class MonoScriptConverter : BundleConverter<MonoScript>
    {

        public override MonoScript? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new MonoScript
            {
                Name = reader.ReadAlignedString()
            };
            if (version.GreaterThanOrEquals(3, 4)) //3.4 and up
            {
                var m_ExecutionOrder = reader.ReadInt32();
            }
            if (version.LessThan(5)) //5.0 down
            {
                var m_PropertiesHash = reader.ReadUInt32();
            }
            else
            {
                var m_PropertiesHash = reader.ReadBytes(16);
            }
            if (version.LessThan(3)) //3.0 down
            {
                var m_PathName = reader.ReadAlignedString();
            }
            res.ClassName = reader.ReadAlignedString();
            if (version.GreaterThanOrEquals(3)) //3.0 and up
            {
                res.NameSpace = reader.ReadAlignedString();
            }
            res.AssemblyName = reader.ReadAlignedString();
            if (version.LessThan(2018, 2)) //2018.2 down
            {
                var m_IsEditorScript = reader.ReadBoolean();
            }
            return res;
        }
    }
}
