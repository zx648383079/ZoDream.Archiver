using System;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class AnimationCurve<T>
    {
        public List<Keyframe<T>> m_Curve;
        public int m_PreInfinity;
        public int m_PostInfinity;
        public int m_RotationOrder;

        public AnimationCurve()
        {
            m_PreInfinity = 2;
            m_PostInfinity = 2;
            m_RotationOrder = 4;
            m_Curve = [];
        }

        public AnimationCurve(IBundleBinaryReader reader, Func<T> readerFunc)
        {
            var version = reader.Get<UnityVersion>();
            int numCurves = reader.ReadInt32();
            m_Curve = [];
            for (int i = 0; i < numCurves; i++)
            {
                m_Curve.Add(new Keyframe<T>(reader, readerFunc));
            }

            m_PreInfinity = reader.ReadInt32();
            m_PostInfinity = reader.ReadInt32();
            if (version.Major > 5 || version.Major == 5 && version.Minor >= 3)//5.3 and up
            {
                m_RotationOrder = reader.ReadInt32();
            }
        }


        private int ToSerializedVersion(int[] version)
        {
            if (version[0] > 2 || version[0] == 2 && version[1] >= 1)
            {
                return 2;
            }
            return 1;
        }
    }

}
