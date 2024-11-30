using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Unity.SerializedFiles;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ClipMuscleConstant : IYamlWriter
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
        public static bool HasShortIndexArray(SerializedType type)
        {
            return Convert.ToHexString(type.OldTypeHash) switch
            {
                "E708B1872AE48FD688AC012DF4A7A178" or "055AA41C7639327940F8900103A10356" => true,
                _ => false
            };
        }
        public ClipMuscleConstant() { }

        public ClipMuscleConstant(UIReader reader)
        {
            var version = reader.Version;
            if (reader.IsLoveAndDeepSpace())
            {
                m_StartX = reader.ReadXForm();
                if (version.GreaterThanOrEquals(5, 5))//5.5 and up
                {
                    m_StopX = reader.ReadXForm();
                }
            }
            else
            {
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
            }
            m_AverageSpeed = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3() : UIReader.Parse(reader.ReadVector4());//5.4 and up
            m_Clip = new Clip(reader);
            m_StartTime = reader.ReadSingle();
            m_StopTime = reader.ReadSingle();
            m_OrientationOffsetY = reader.ReadSingle();
            m_Level = reader.ReadSingle();
            m_CycleOffset = reader.ReadSingle();
            m_AverageAngularSpeed = reader.ReadSingle();

            if (reader.IsSR() && HasShortIndexArray(reader.SerializedType))
            {
                m_IndexArray = reader.ReadArray(r => (int)r.ReadInt16());
            }
            else
            {
                m_IndexArray = reader.ReadArray(r => r.ReadInt32());
            }
            if (version.LessThan(4, 3)) //4.3 down
            {
                var m_AdditionalCurveIndexArray = reader.ReadArray(r => r.ReadInt32());
            }
            int numDeltas = reader.ReadInt32();
            m_ValueArrayDelta = new List<ValueDelta>();
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
        public static ClipMuscleConstant ParseGI(UIReader reader)
        {
            var version = reader.Version;
            var clipMuscleConstant = new ClipMuscleConstant();

            clipMuscleConstant.m_DeltaPose = HumanPose.ParseGI(reader);
            clipMuscleConstant.m_StartX = UIReader.Parse(reader.ReadXForm4());
            clipMuscleConstant.m_StopX = UIReader.Parse(reader.ReadXForm4());
            clipMuscleConstant.m_LeftFootStartX = UIReader.Parse(reader.ReadXForm4());
            clipMuscleConstant.m_RightFootStartX = UIReader.Parse(reader.ReadXForm4());

            clipMuscleConstant.m_AverageSpeed = UIReader.Parse(reader.ReadVector4());

            clipMuscleConstant.m_Clip = Clip.ParseGI(reader);

            clipMuscleConstant.m_StartTime = reader.ReadSingle();
            clipMuscleConstant.m_StopTime = reader.ReadSingle();
            clipMuscleConstant.m_OrientationOffsetY = reader.ReadSingle();
            clipMuscleConstant.m_Level = reader.ReadSingle();
            clipMuscleConstant.m_CycleOffset = reader.ReadSingle();
            clipMuscleConstant.m_AverageAngularSpeed = reader.ReadSingle();

            clipMuscleConstant.m_IndexArray = reader.ReadArray(0xC8, r => (int)r.ReadInt16());

            var valueArrayDeltaCount = (int)reader.ReadUInt64();
            var valueArrayDeltaOffset = reader.Position + reader.ReadInt64();

            if (valueArrayDeltaOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var valueArrayReferencePoseCount = (int)reader.ReadUInt64();
            var valueArrayReferencePoseOffset = reader.Position + reader.ReadInt64();

            if (valueArrayReferencePoseOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            clipMuscleConstant.m_Mirror = reader.ReadBoolean();
            clipMuscleConstant.m_LoopTime = reader.ReadBoolean();
            clipMuscleConstant.m_LoopBlend = reader.ReadBoolean();
            clipMuscleConstant.m_LoopBlendOrientation = reader.ReadBoolean();
            clipMuscleConstant.m_LoopBlendPositionY = reader.ReadBoolean();
            clipMuscleConstant.m_LoopBlendPositionXZ = reader.ReadBoolean();
            clipMuscleConstant.m_StartAtOrigin = reader.ReadBoolean();
            clipMuscleConstant.m_KeepOriginalOrientation = reader.ReadBoolean();
            clipMuscleConstant.m_KeepOriginalPositionY = reader.ReadBoolean();
            clipMuscleConstant.m_KeepOriginalPositionXZ = reader.ReadBoolean();
            clipMuscleConstant.m_HeightFromFeet = reader.ReadBoolean();
            reader.AlignStream();

            if (valueArrayDeltaCount > 0)
            {
                reader.Position = valueArrayDeltaOffset;
                clipMuscleConstant.m_ValueArrayDelta = new List<ValueDelta>();
                for (int i = 0; i < valueArrayDeltaCount; i++)
                {
                    clipMuscleConstant.m_ValueArrayDelta.Add(new ValueDelta(reader));
                }
            }

            if (valueArrayReferencePoseCount > 0)
            {
                reader.Position = valueArrayReferencePoseOffset;
                clipMuscleConstant.m_ValueArrayReferencePose = reader.ReadArray(valueArrayReferencePoseCount, r => r.ReadSingle());
            }

            return clipMuscleConstant;
        }
        //public YAMLNode ExportYAML(int[] version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.AddSerializedVersion(ToSerializedVersion(version));
        //    node.Add(nameof(m_StartTime), m_StartTime);
        //    node.Add(nameof(m_StopTime), m_StopTime);
        //    node.Add(nameof(m_OrientationOffsetY), m_OrientationOffsetY);
        //    node.Add(nameof(m_Level), m_Level);
        //    node.Add(nameof(m_CycleOffset), m_CycleOffset);
        //    node.Add(nameof(m_LoopTime), m_LoopTime);
        //    node.Add(nameof(m_LoopBlend), m_LoopBlend);
        //    node.Add(nameof(m_LoopBlendOrientation), m_LoopBlendOrientation);
        //    node.Add(nameof(m_LoopBlendPositionY), m_LoopBlendPositionY);
        //    node.Add(nameof(m_LoopBlendPositionXZ), m_LoopBlendPositionXZ);
        //    node.Add(nameof(m_KeepOriginalOrientation), m_KeepOriginalOrientation);
        //    node.Add(nameof(m_KeepOriginalPositionY), m_KeepOriginalPositionY);
        //    node.Add(nameof(m_KeepOriginalPositionXZ), m_KeepOriginalPositionXZ);
        //    node.Add(nameof(m_HeightFromFeet), m_HeightFromFeet);
        //    node.Add(nameof(m_Mirror), m_Mirror);
        //    return node;
        //}
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
