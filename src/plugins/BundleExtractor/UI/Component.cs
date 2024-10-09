using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoDream.BundleExtractor.SerializedFiles;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.UI
{
    public abstract class UIComponent : EditorExtension
    {
        public PPtr<GameObject> m_GameObject;

        protected UIComponent(UIReader reader)
            : base(reader)
        {
            m_GameObject = new PPtr<GameObject>(reader);
        }
    }
}
