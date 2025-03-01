using System.Collections.Generic;
using System.Numerics;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ClipMuscleConstant : IElementLoader
    {
        public HumanPose m_DeltaPose;
        public XForm<Vector3> m_StartX;
        public XForm<Vector3> m_StopX;
        public XForm<Vector3> m_LeftFootStartX;
        public XForm<Vector3> m_RightFootStartX;
        public XForm<Vector3> m_MotionStartX;
        public XForm<Vector3> m_MotionStopX;
        public Vector3 m_AverageSpeed;
        public Clip m_Clip;
        public float m_StartTime;
        public float m_StopTime;
        public float m_OrientationOffsetY;
        public float m_Level;
        public float m_CycleOffset;
        public float m_AverageAngularSpeed;
        public int[] m_IndexArray;
        public List<ValueDelta> m_ValueArrayDelta;
        public float[] m_ValueArrayReferencePose;
        public bool m_Mirror;
        public bool m_LoopTime;
        public bool m_LoopBlend;
        public bool m_LoopBlendOrientation;
        public bool m_LoopBlendPositionY;
        public bool m_LoopBlendPositionXZ;
        public bool m_StartAtOrigin;
        public bool m_KeepOriginalOrientation;
        public bool m_KeepOriginalPositionY;
        public bool m_KeepOriginalPositionXZ;
        public bool m_HeightFromFeet;
       

        public void ReadBase(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            if (version.LessThan(4, 3)) //4.3 down
            {
                var m_AdditionalCurveIndexArray = reader.ReadArray(r => r.ReadInt32());
            }
            int numDeltas = reader.ReadInt32();
            m_ValueArrayDelta = [];
            for (int i = 0; i < numDeltas; i++)
            {
                m_ValueArrayDelta.Add(new ValueDelta(reader));
            }
            if (version.GreaterThanOrEquals(5, 3))//5.3 and up
            {
                m_ValueArrayReferencePose = reader.ReadArray(r => r.ReadSingle());
            }

            m_Mirror = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_LoopTime = reader.ReadBoolean();
            }
            m_LoopBlend = reader.ReadBoolean();
            m_LoopBlendOrientation = reader.ReadBoolean();
            m_LoopBlendPositionY = reader.ReadBoolean();
            m_LoopBlendPositionXZ = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(5, 5))//5.5 and up
            {
                m_StartAtOrigin = reader.ReadBoolean();
            }
            m_KeepOriginalOrientation = reader.ReadBoolean();
            m_KeepOriginalPositionY = reader.ReadBoolean();
            m_KeepOriginalPositionXZ = reader.ReadBoolean();
            m_HeightFromFeet = reader.ReadBoolean();
            reader.AlignStream();
        }

        public void Read(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            m_DeltaPose = new HumanPose(reader);
            m_StartX = reader.ReadXForm();
            if (version.GreaterThanOrEquals(5, 5))//5.5 and up
            {
                m_StopX = reader.ReadXForm();
            }
            m_LeftFootStartX = reader.ReadXForm();
            m_RightFootStartX = reader.ReadXForm();
            if (version.LessThan(5))//5.0 down
            {
                m_MotionStartX = reader.ReadXForm();
                m_MotionStopX = reader.ReadXForm();
            }
            m_AverageSpeed = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3Or4() : 
                UnityReaderExtension.Parse(reader.ReadVector4());//5.4 and up
            m_Clip = new Clip();
            reader.Get<IBundleElementScanner>().TryRead(reader, m_Clip);
            m_StartTime = reader.ReadSingle();
            m_StopTime = reader.ReadSingle();
            m_OrientationOffsetY = reader.ReadSingle();
            m_Level = reader.ReadSingle();
            m_CycleOffset = reader.ReadSingle();
            m_AverageAngularSpeed = reader.ReadSingle();

            m_IndexArray = reader.ReadArray(r => r.ReadInt32());
            ReadBase(reader);
        }


        private int ToSerializedVersion(int[] version)
        {
            if (version[0] > 5 || version[0] == 5 && version[1] >= 6)
            {
                return 3;
            }
            else if (version[0] > 4 || version[0] == 4 && version[1] >= 3)
            {
                return 2;
            }
            return 1;
        }
    }
}
