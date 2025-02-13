﻿using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class FloatCurve
    {
        public AnimationCurve<float> curve;
        public string attribute;
        public string path;
        public ElementIDType classID;
        public PPtr<MonoScript> script;
        public int flags;

        public FloatCurve(string path, string attribute, ElementIDType classID, PPtr<MonoScript> script)
        {
            curve = new AnimationCurve<float>();
            this.attribute = attribute;
            this.path = path;
            this.classID = classID;
            this.script = script;
            flags = 0;
        }

        public FloatCurve(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            curve = new AnimationCurve<float>(reader, reader.ReadSingle);
            attribute = reader.ReadAlignedString();
            path = reader.ReadAlignedString();
            classID = (ElementIDType)reader.ReadInt32();
            script = new PPtr<MonoScript>(reader);
            if (version.Major == 2022 && version.Minor >= 2) //2022.2 and up
            {
                flags = reader.ReadInt32();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is FloatCurve floatCurve)
            {
                return attribute == floatCurve.attribute && path == floatCurve.path && classID == floatCurve.classID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            unchecked
            {
                hash = hash * 23 + path.GetHashCode();
            }
            return hash;
        }
    }

}
