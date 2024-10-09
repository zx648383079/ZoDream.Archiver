using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoDream.BundleExtractor.UI
{
    public class NamedObject : EditorExtension
    {
        public string m_Name;

        public override string Name => m_Name;

        protected NamedObject(UIReader reader) : base(reader)
        {
            m_Name = reader.ReadAlignedString();
        }
    }
}
