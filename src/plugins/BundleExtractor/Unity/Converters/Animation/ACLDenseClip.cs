using System;
using System.Collections.Generic;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ACLDenseClipConverter : BundleConverter<ACLDenseClip>
    {
        public override ACLDenseClip? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new ACLDenseClip();
            ReadBase(res, reader, serializer, () => {
            });
            return res;
        }

        public static void ReadBase(ACLDenseClip res, IBundleBinaryReader reader, IBundleSerializer serializer, Action cb)
        {
            UnityConverter.ReadDenseClip(res, reader, serializer);
            res.ACLType = reader.ReadInt32();
            cb.Invoke();
            Process(res);
        }

        private static void Process(ACLDenseClip res)
        {
            if (res.ACLType == 0 || res.SampleArray is not null && res.SampleArray.Length > 0)
            {
                return;
            }

            var sampleArray = new List<float>();

            var size = res.ACLType >> 2;
            var factor = (float)((1 << res.ACLType) - 1);
            var aclSpan = res.ACLArray.To4bArray().AsSpan();
            var buffer = (stackalloc byte[8]);

            for (int i = 0; i < res.FrameCount; i++)
            {
                var index = i * (int)(res.CurveCount * size);
                for (int j = 0; j < res.nPositionCurves; j++)
                {
                    sampleArray.Add(ReadCurve(res, aclSpan, res.PositionFactor, ref index));
                }
                for (int j = 0; j < res.nRotationCurves; j++)
                {
                    sampleArray.Add(ReadCurve(res, aclSpan, 1.0f, ref index));
                }
                for (int j = 0; j < res.nEulerCurves; j++)
                {
                    sampleArray.Add(ReadCurve(res, aclSpan, res.EulerFactor, ref index));
                }
                for (int j = 0; j < res.nScaleCurves; j++)
                {
                    sampleArray.Add(ReadCurve(res, aclSpan, res.ScaleFactor, ref index));
                }
                var m_nFloatCurves = res.CurveCount - (res.nPositionCurves + res.nRotationCurves + res.nEulerCurves + res.nScaleCurves + res.nGenericCurves);
                for (int j = 0; j < m_nFloatCurves; j++)
                {
                    sampleArray.Add(ReadCurve(res, aclSpan, res.FloatFactor, ref index));
                }
            }

            res.SampleArray = sampleArray.ToArray();
        }

        private static float ReadCurve(ACLDenseClip res, Span<byte> aclSpan, float curveFactor, ref int curveIndex)
        {
            var buffer = (stackalloc byte[8]);

            var curveSize = res.ACLType >> 2;
            var factor = (float)((1 << res.ACLType) - 1);

            aclSpan.Slice(curveIndex, curveSize).CopyTo(buffer);
            var temp = buffer.ToArray().From4bToArray(0, curveSize);
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

        
    }
}
