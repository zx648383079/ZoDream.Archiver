using System;
using System.Collections.Generic;
using System.IO;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{

    internal sealed class AnimationClip(UIReader reader) : NamedObject(reader), IFileWriter
    {
        public AnimationType m_AnimationType;
        public bool m_Legacy;
        public bool m_Compressed;
        public bool m_UseHighQualityCurve;
        public List<QuaternionCurve> m_RotationCurves;
        public List<CompressedAnimationCurve> m_CompressedRotationCurves;
        public List<Vector3Curve> m_EulerCurves;
        public List<Vector3Curve> m_PositionCurves;
        public List<Vector3Curve> m_ScaleCurves;
        public List<FloatCurve> m_FloatCurves;
        public List<PPtrCurve> m_PPtrCurves;
        public float m_SampleRate;
        public int m_WrapMode;
        public AABB m_Bounds;
        public uint m_MuscleClipSize;
        public ClipMuscleConstant m_MuscleClip;
        public AnimationClipBindingConstant m_ClipBindingConstant;
        public List<AnimationEvent> m_Events;
        public StreamingInfo m_StreamData;

        private bool hasStreamingInfo = false;

        public void ReadBase(IBundleBinaryReader reader, Action cb)
        {
            base.Read(reader);
            var version = reader.Get<UnityVersion>();
            if (version.Major >= 5)//5.0 and up
            {
                m_Legacy = reader.ReadBoolean();
            }
            else if (version.Major >= 4)//4.0 and up
            {
                m_AnimationType = (AnimationType)reader.ReadInt32();
                if (m_AnimationType == AnimationType.Legacy)
                {
                    m_Legacy = true;
                }
            }
            else
            {
                m_Legacy = true;
            }
            cb.Invoke();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_ClipBindingConstant = new AnimationClipBindingConstant(_reader);
            }
            if (version.GreaterThanOrEquals(2018, 3)) //2018.3 and up
            {
                var m_HasGenericRootTransform = reader.ReadBoolean();
                var m_HasMotionFloatCurves = reader.ReadBoolean();
                reader.AlignStream();
            }
            int numEvents = reader.ReadInt32();
            m_Events = [];
            for (int i = 0; i < numEvents; i++)
            {
                m_Events.Add(new AnimationEvent(_reader));
            }
            if (version.Major >= 2017) //2017 and up
            {
                reader.AlignStream();
            }
            
        }

        public override void Read(IBundleBinaryReader reader)
        {
            ReadBase(reader, () => {
                var version = reader.Get<UnityVersion>();
                m_Compressed = reader.ReadBoolean();
                if (version.GreaterThanOrEquals(4, 3))//4.3 and up
                {
                    m_UseHighQualityCurve = reader.ReadBoolean();
                }
                reader.AlignStream();
                int numRCurves = reader.ReadInt32();
                m_RotationCurves = [];
                for (int i = 0; i < numRCurves; i++)
                {
                    m_RotationCurves.Add(new QuaternionCurve(reader));
                }

                int numCRCurves = reader.ReadInt32();
                m_CompressedRotationCurves = [];
                for (int i = 0; i < numCRCurves; i++)
                {
                    m_CompressedRotationCurves.Add(new CompressedAnimationCurve(reader));
                }


                if (version.GreaterThanOrEquals(5, 3))//5.3 and up
                {
                    int numEulerCurves = reader.ReadInt32();
                    m_EulerCurves = [];
                    for (int i = 0; i < numEulerCurves; i++)
                    {
                        m_EulerCurves.Add(new Vector3Curve(reader));
                    }
                }

                int numPCurves = reader.ReadInt32();
                m_PositionCurves = [];
                for (int i = 0; i < numPCurves; i++)
                {
                    m_PositionCurves.Add(new Vector3Curve(reader));
                }

                int numSCurves = reader.ReadInt32();
                m_ScaleCurves = [];
                for (int i = 0; i < numSCurves; i++)
                {
                    m_ScaleCurves.Add(new Vector3Curve(reader));
                }

                int numFCurves = reader.ReadInt32();
                m_FloatCurves = [];
                for (int i = 0; i < numFCurves; i++)
                {
                    m_FloatCurves.Add(new FloatCurve(reader));
                }

                if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
                {
                    int numPtrCurves = reader.ReadInt32();
                    m_PPtrCurves = [];
                    for (int i = 0; i < numPtrCurves; i++)
                    {
                        m_PPtrCurves.Add(new PPtrCurve(reader));
                    }
                }

                m_SampleRate = reader.ReadSingle();
                m_WrapMode = reader.ReadInt32();
                if (version.GreaterThanOrEquals(3, 4)) //3.4 and up
                {
                    m_Bounds = new AABB(reader);
                }
                if (version.Major >= 4)//4.0 and up
                {
                    m_MuscleClipSize = reader.ReadUInt32();
                    m_MuscleClip = new();
                    reader.Get<IBundleElementScanner>().TryRead(reader, m_MuscleClip);
                }
            });
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            // TODO
        }
    }
}
