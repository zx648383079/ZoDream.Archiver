﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class MonoBehavior(UIReader reader) : UIBehavior(reader), IFileWriter
    {
        public PPtr<MonoScript> m_Script;
        public string m_Name;

        public override string Name => string.IsNullOrEmpty(m_Name) ? m_Script.Name : m_Name;


        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_Script = new PPtr<MonoScript>(reader);
            m_Name = reader.ReadAlignedString();
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            // TODO
        }
    }
}
