using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class NamedObject : EditorExtension
    {
        public string m_Name;

        public override string Name => m_Name;

        protected NamedObject(UIReader reader) : 
            this(reader, true)
        {
            
        }

        protected NamedObject(UIReader reader, bool isReadable)
            : base(reader, isReadable)
        {
            if (isReadable)
            {
                m_Name = reader.ReadAlignedString();
            }
        }
    }
}
