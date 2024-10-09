using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoDream.BundleExtractor.SerializedFiles;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.UI
{
    public abstract class UIBehaviour : UIComponent
    {
        public byte m_Enabled;

        protected UIBehaviour(UIReader reader) 
            : base(reader)
        {
            m_Enabled = reader.Reader.ReadByte();
            reader.Reader.AlignStream();
        }
    }
}
