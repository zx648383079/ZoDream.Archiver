using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class MonoScript(UIReader reader) : NamedObject(reader)
    {
        public string m_ClassName;
        public string m_Namespace;
        public string m_AssemblyName;

        public override string Name => string.IsNullOrEmpty(m_Name) ? m_ClassName : m_Name;


        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var version = reader.Get<UnityVersion>();
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
            m_ClassName = reader.ReadAlignedString();
            if (version.GreaterThanOrEquals(3)) //3.0 and up
            {
                m_Namespace = reader.ReadAlignedString();
            }
            m_AssemblyName = reader.ReadAlignedString();
            if (version.LessThan(2018, 2)) //2018.2 down
            {
                var m_IsEditorScript = reader.ReadBoolean();
            }
        }
    }
}
