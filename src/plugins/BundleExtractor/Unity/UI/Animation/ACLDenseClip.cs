using System;
using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ACLDenseClip : DenseClip
    {
        public int m_ACLType;
        public byte[] m_ACLArray;
        public float m_PositionFactor;
        public float m_EulerFactor;
        public float m_ScaleFactor;
        public float m_FloatFactor;
        public uint m_nPositionCurves;
        public uint m_nRotationCurves;
        public uint m_nEulerCurves;
        public uint m_nScaleCurves;
        public uint m_nGenericCurves;

        public override void Read(IBundleBinaryReader reader)
        {
            ReadBase(reader, () => {
            });
        }

        public void ReadBase(IBundleBinaryReader reader, Action cb)
        {
            base.Read(reader);
            m_ACLType = reader.ReadInt32();
            
            Process();
        }

        private void Process()
        {
            if (m_ACLType == 0 || m_SampleArray is not null && m_SampleArray.Length > 0)
            {
                return;
            }

            var sampleArray = new List<float>();

            var size = m_ACLType >> 2;
            var factor = (float)((1 << m_ACLType) - 1);
            var aclSpan = ToUInt4Array(m_ACLArray).AsSpan();
            var buffer = (stackalloc byte[8]);

            for (int i = 0; i < m_FrameCount; i++)
            {
                var index = i * (int)(m_CurveCount * size);
                for (int j = 0; j < m_nPositionCurves; j++)
                {
                    sampleArray.Add(ReadCurve(aclSpan, m_PositionFactor, ref index));
                }
                for (int j = 0; j < m_nRotationCurves; j++)
                {
                    sampleArray.Add(ReadCurve(aclSpan, 1.0f, ref index));
                }
                for (int j = 0; j < m_nEulerCurves; j++)
                {
                    sampleArray.Add(ReadCurve(aclSpan, m_EulerFactor, ref index));
                }
                for (int j = 0; j < m_nScaleCurves; j++)
                {
                    sampleArray.Add(ReadCurve(aclSpan, m_ScaleFactor, ref index));
                }
                var m_nFloatCurves = m_CurveCount - (m_nPositionCurves + m_nRotationCurves + m_nEulerCurves + m_nScaleCurves + m_nGenericCurves);
                for (int j = 0; j < m_nFloatCurves; j++)
                {
                    sampleArray.Add(ReadCurve(aclSpan, m_FloatFactor, ref index));
                }
            }

            m_SampleArray = sampleArray.ToArray();
        }

        private float ReadCurve(Span<byte> aclSpan, float curveFactor, ref int curveIndex)
        {
            var buffer = (stackalloc byte[8]);

            var curveSize = m_ACLType >> 2;
            var factor = (float)((1 << m_ACLType) - 1);

            aclSpan.Slice(curveIndex, curveSize).CopyTo(buffer);
            var temp = ToUInt8Array(buffer.ToArray(), 0, curveSize);
            buffer.Clear();
            temp.CopyTo(buffer);

            float curve;
            var value = BitConverter.ToUInt64(buffer);
            if (value != 0)
            {
                curve = (value / factor - 0.5f) * 2;
            }
            else
            {
                curve = -1.0f;
            }

            curve *= curveFactor;
            curveIndex += curveSize;

            return curve;
        }

        public static byte[] ToUInt4Array(byte[] source) => ToUInt4Array(source, 0, source.Length);
        public static byte[] ToUInt4Array(byte[] source, int offset, int size)
        {
            var buffer = new byte[size * 2];
            for (var i = 0; i < size; i++)
            {
                var idx = i * 2;
                buffer[idx] = (byte)(source[offset + i] >> 4);
                buffer[idx + 1] = (byte)(source[offset + i] & 0xF);
            }
            return buffer;
        }
        public static byte[] ToUInt8Array(byte[] source, int offset, int size)
        {
            var buffer = new byte[size / 2];
            for (var i = 0; i < size; i++)
            {
                var idx = i / 2;
                if (i % 2 == 0)
                {
                    buffer[idx] = (byte)(source[offset + i] << 4);
                }
                else
                {
                    buffer[idx] |= source[offset + i];
                }
            }
            return buffer;
        }
    }
}
