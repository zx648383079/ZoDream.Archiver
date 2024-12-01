﻿using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class Entertainment24ElementScanner(string package) : IBundleElementScanner
    {

        public bool IsNaraka => package.Contains("naraka");

        public bool TryRead(IBundleBinaryReader reader, object instance)
        {
            if (IsNaraka && instance is Material m)
            {
                CreateInstance(reader, m);
                return true;
            }
            if (instance is IElementLoader l)
            {
                l.Read(reader);
                return true;
            }
            return false;
        }

        private void CreateInstance(IBundleBinaryReader reader, Material instance)
        {
            instance.ReadBase(reader);
            var version = reader.Get<UnityVersion>();

            if (version.GreaterThanOrEquals(5, 1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }

            var value = reader.ReadInt32();

            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadArray(r => r.ReadString());
            }

            instance.m_SavedProperties = new UnityPropertySheet(reader);
        }
    }
}
