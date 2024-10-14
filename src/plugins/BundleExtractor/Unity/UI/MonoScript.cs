using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public sealed class MonoScript : NamedObject
    {
        public string m_ClassName;
        public string m_Namespace;
        public string m_AssemblyName;

        public override string Name => string.IsNullOrEmpty(m_Name) ? m_ClassName : m_Name;

        public MonoScript(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            if (version.GreaterThanOrEquals(3, 4)) //3.4 and up
            {
                var m_ExecutionOrder = reader.Reader.ReadInt32();
            }
            if (version.LessThan(5)) //5.0 down
            {
                var m_PropertiesHash = reader.Reader.ReadUInt32();
            }
            else
            {
                var m_PropertiesHash = reader.Reader.ReadBytes(16);
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
                var m_IsEditorScript = reader.Reader.ReadBoolean();
            }
        }
    }
}
