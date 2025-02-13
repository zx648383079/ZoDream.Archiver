﻿using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.Exporters;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class MonoBehavior(UIReader reader) : UIBehavior(reader), IFileExporter
    {
        public PPtr<MonoScript> m_Script;
        public string m_Name;

        public override string Name => string.IsNullOrEmpty(m_Name) ? m_Script.Name : m_Name;

        public UnityVersion Version => _reader.Version;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_Script = new PPtr<MonoScript>(reader);
            m_Name = reader.ReadAlignedString();
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (m_Script is null || !m_Script.TryGet(out var script))
            {
                return;
            }
            switch (script.m_ClassName)
            {
                case "CubismMoc":
                    new CubismExporter(_reader).SaveAs(fileName, mode);
                    break;
                case "CubismPhysicsController":
                    break;
                case "CubismExpressionData":
                    break;
                case "CubismFadeMotionData":
                    break;
                case "CubismFadeMotionList":
                    break;
                case "CubismEyeBlinkParameter":
                    break;
                case "CubismMouthParameter":
                    break;
                case "CubismParameter":
                    break;
                case "CubismPart":
                    break;
                case "CubismDisplayInfoParameterName":
                    break;
                case "CubismDisplayInfoPartName":
                    break;
            }
        }
    }
}
