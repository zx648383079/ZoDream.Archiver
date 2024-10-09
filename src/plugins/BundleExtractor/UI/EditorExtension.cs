using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.SerializedFiles;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.UI
{
    public abstract class EditorExtension : UIObject
    {
        protected EditorExtension(UIReader reader) : base(reader)
        {
            if (reader.Platform == BuildTarget.NoTarget)
            {
                var m_PrefabParentObject = new PPtr<EditorExtension>(reader);
                var m_PrefabInternal = new PPtr<UIObject>(reader); //PPtr<Prefab>
            }
        }
    }
}
