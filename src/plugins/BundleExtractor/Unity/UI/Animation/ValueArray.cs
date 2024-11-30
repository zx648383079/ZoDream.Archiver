using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ValueArray
    {
        public bool[] m_BoolValues;
        public int[] m_IntValues;
        public float[] m_FloatValues;
        public Vector4[] m_VectorValues;
        public Vector3[] m_PositionValues;
        public Vector4[] m_QuaternionValues;
        public Vector3[] m_ScaleValues;

        public ValueArray(UIReader reader)
        {
            var version = reader.Version;

            if (version.LessThan(5, 5)) //5.5 down
            {
                m_BoolValues = reader.ReadArray(r => r.ReadBoolean());
                reader.AlignStream();
                m_IntValues = reader.ReadInt32Array();
                m_FloatValues = reader.ReadArray(r => r.ReadSingle());
            }

            if (version.GreaterThan(4, 3)) //4.3 down
            {
                m_VectorValues = reader.ReadArray(_ => reader.ReadVector4());
            }
            else
            {
                m_PositionValues = reader.ReadVector3Array();

                m_QuaternionValues = reader.ReadArray(_ => reader.ReadVector4());

                m_ScaleValues = reader.ReadVector3Array();

                if (version.GreaterThanOrEquals(5, 5)) //5.5 and up
                {
                    m_FloatValues = reader.ReadArray(r => r.ReadSingle());
                    m_IntValues = reader.ReadInt32Array();
                    m_BoolValues = reader.ReadArray(r => r.ReadBoolean());
                    reader.AlignStream();
                }
            }
        }
    }

}
