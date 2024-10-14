using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class Keyframe<T> : IYamlWriter
    {
        public float time;
        public T value;
        public T inSlope;
        public T outSlope;
        public int weightedMode;
        public T inWeight;
        public T outWeight;

        public Keyframe(float time, T value, T inSlope, T outSlope, T weight)
        {
            this.time = time;
            this.value = value;
            this.inSlope = inSlope;
            this.outSlope = outSlope;
            weightedMode = 0;
            inWeight = weight;
            outWeight = weight;
        }

        public Keyframe(UIReader reader, Func<T> readerFunc)
        {
            time = reader.Reader.ReadSingle();
            value = readerFunc();
            inSlope = readerFunc();
            outSlope = readerFunc();
            if (reader.Version.Major >= 2018) //2018 and up
            {
                weightedMode = reader.Reader.ReadInt32();
                inWeight = readerFunc();
                outWeight = readerFunc();
            }
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.AddSerializedVersion(ToSerializedVersion(version));
        //    node.Add(nameof(time), time);
        //    node.Add(nameof(value), value.ExportYAML(version));
        //    node.Add(nameof(inSlope), inSlope.ExportYAML(version));
        //    node.Add(nameof(outSlope), outSlope.ExportYAML(version));
        //    if (version.GreaterThanOrEquals(2018)) //2018 and up
        //    {
        //        node.Add(nameof(weightedMode), weightedMode);
        //        node.Add(nameof(inWeight), inWeight.ExportYAML(version));
        //        node.Add(nameof(outWeight), outWeight.ExportYAML(version));
        //    }
        //    return node;
        //}

        private int ToSerializedVersion(UnityVersion version)
        {
            if (version.GreaterThanOrEquals(2018)) //2018 and up
            {
                return 3;
            }
            else if (version.GreaterThanOrEquals(5, 5))
            {
                return 2;
            }
            return 1;
        }
    }

    public class AnimationCurve<T> : IYamlWriter
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
            m_Curve = new List<Keyframe<T>>();
        }

        public AnimationCurve(UIReader reader, Func<T> readerFunc)
        {
            var version = reader.Version;
            int numCurves = reader.Reader.ReadInt32();
            m_Curve = new List<Keyframe<T>>();
            for (int i = 0; i < numCurves; i++)
            {
                m_Curve.Add(new Keyframe<T>(reader, readerFunc));
            }

            m_PreInfinity = reader.Reader.ReadInt32();
            m_PostInfinity = reader.Reader.ReadInt32();
            if (version.Major > 5 || version.Major == 5 && version.Minor >= 3)//5.3 and up
            {
                m_RotationOrder = reader.Reader.ReadInt32();
            }
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.AddSerializedVersion(ToSerializedVersion(version));
        //    node.Add(nameof(m_Curve), m_Curve.ExportYAML(version));
        //    node.Add(nameof(m_PreInfinity), m_PreInfinity);
        //    node.Add(nameof(m_PostInfinity), m_PostInfinity);
        //    if (version.GreaterThanOrEquals(5,3))//5.3 and up
        //    {
        //        node.Add(nameof(m_RotationOrder), m_RotationOrder);
        //    }
        //    return node;
        //}

        private int ToSerializedVersion(int[] version)
        {
            if (version[0] > 2 || version[0] == 2 && version[1] >= 1)
            {
                return 2;
            }
            return 1;
        }
    }

    public class QuaternionCurve : IYamlWriter
    {
        public AnimationCurve<Vector4> curve;
        public string path;

        public QuaternionCurve(string path)
        {
            curve = new AnimationCurve<Vector4>();
            this.path = path;
        }

        public QuaternionCurve(UIReader reader)
        {
            curve = new AnimationCurve<Vector4>(reader, reader.ReadVector4);
            path = reader.ReadAlignedString();
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    YAMLMappingNode node = new YAMLMappingNode();
        //    node.Add(nameof(curve), curve.ExportYAML(version));
        //    node.Add(nameof(path), path);
        //    return node;
        //}
        public override bool Equals(object obj)
        {
            if (obj is QuaternionCurve quaternionCurve)
            {
                return path == quaternionCurve.path;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 199;
            unchecked
            {
                hash = 617 + hash * path.GetHashCode();
            }
            return hash;
        }
    }

    public class PackedFloatVector : IYamlWriter
    {
        public uint m_NumItems;
        public float m_Range;
        public float m_Start;
        public byte[] m_Data;
        public byte m_BitSize;

        public PackedFloatVector(UIReader reader)
        {
            m_NumItems = reader.Reader.ReadUInt32();
            m_Range = reader.Reader.ReadSingle();
            m_Start = reader.Reader.ReadSingle();

            int numData = reader.Reader.ReadInt32();
            m_Data = reader.Reader.ReadBytes(numData);
            reader.Reader.AlignStream();

            m_BitSize = reader.Reader.ReadByte();
            reader.Reader.AlignStream();
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(m_NumItems), m_NumItems);
        //    node.Add(nameof(m_Range), m_Range);
        //    node.Add(nameof(m_Start), m_Start);
        //    node.Add(nameof(m_Data), m_Data.ExportYAML());
        //    node.Add(nameof(m_BitSize), m_BitSize);
        //    return node;
        //}

        public float[] UnpackFloats(int itemCountInChunk, int chunkStride, int start = 0, int numChunks = -1)
        {
            int bitPos = m_BitSize * start;
            int indexPos = bitPos / 8;
            bitPos %= 8;

            float scale = 1.0f / m_Range;
            if (numChunks == -1)
                numChunks = (int)m_NumItems / itemCountInChunk;
            var end = chunkStride * numChunks / 4;
            var data = new List<float>();
            for (var index = 0; index != end; index += chunkStride / 4)
            {
                for (int i = 0; i < itemCountInChunk; ++i)
                {
                    uint x = 0;

                    int bits = 0;
                    while (bits < m_BitSize)
                    {
                        x |= (uint)(m_Data[indexPos] >> bitPos << bits);
                        int num = Math.Min(m_BitSize - bits, 8 - bitPos);
                        bitPos += num;
                        bits += num;
                        if (bitPos == 8)
                        {
                            indexPos++;
                            bitPos = 0;
                        }
                    }
                    x &= (uint)(1 << m_BitSize) - 1u;
                    data.Add(x / (scale * ((1 << m_BitSize) - 1)) + m_Start);
                }
            }

            return data.ToArray();
        }
    }

    public class PackedIntVector : IYamlWriter
    {
        public uint m_NumItems;
        public byte[] m_Data;
        public byte m_BitSize;

        public PackedIntVector(UIReader reader)
        {
            m_NumItems = reader.Reader.ReadUInt32();

            int numData = reader.Reader.ReadInt32();
            m_Data = reader.Reader.ReadBytes(numData);
            reader.Reader.AlignStream();

            m_BitSize = reader.Reader.ReadByte();
            reader.Reader.AlignStream();
        }
        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(m_NumItems), m_NumItems);
        //    node.Add(nameof(m_Data), m_Data.ExportYAML());
        //    node.Add(nameof(m_BitSize), m_BitSize);
        //    return node;
        //}

        public int[] UnpackInts()
        {
            var data = new int[m_NumItems];
            int indexPos = 0;
            int bitPos = 0;
            for (int i = 0; i < m_NumItems; i++)
            {
                int bits = 0;
                data[i] = 0;
                while (bits < m_BitSize)
                {
                    data[i] |= m_Data[indexPos] >> bitPos << bits;
                    int num = Math.Min(m_BitSize - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8)
                    {
                        indexPos++;
                        bitPos = 0;
                    }
                }
                data[i] &= (1 << m_BitSize) - 1;
            }
            return data;
        }
    }

    public class PackedQuatVector : IYamlWriter
    {
        public uint m_NumItems;
        public byte[] m_Data;

        public PackedQuatVector(UIReader reader)
        {
            m_NumItems = reader.Reader.ReadUInt32();

            int numData = reader.Reader.ReadInt32();
            m_Data = reader.Reader.ReadBytes(numData);

            reader.Reader.AlignStream();
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(m_NumItems), m_NumItems);
        //    node.Add(nameof(m_Data), m_Data.ExportYAML());
        //    return node;
        //}

        public Quaternion[] UnpackQuats()
        {
            var data = new Quaternion[m_NumItems];
            int indexPos = 0;
            int bitPos = 0;

            for (int i = 0; i < m_NumItems; i++)
            {
                uint flags = 0;

                int bits = 0;
                while (bits < 3)
                {
                    flags |= (uint)(m_Data[indexPos] >> bitPos << bits);
                    int num = Math.Min(3 - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8)
                    {
                        indexPos++;
                        bitPos = 0;
                    }
                }
                flags &= 7;


                var q = new Quaternion();
                float sum = 0;
                for (int j = 0; j < 4; j++)
                {
                    if ((flags & 3) != j)
                    {
                        int bitSize = ((flags & 3) + 1) % 4 == j ? 9 : 10;
                        uint x = 0;

                        bits = 0;
                        while (bits < bitSize)
                        {
                            x |= (uint)(m_Data[indexPos] >> bitPos << bits);
                            int num = Math.Min(bitSize - bits, 8 - bitPos);
                            bitPos += num;
                            bits += num;
                            if (bitPos == 8)
                            {
                                indexPos++;
                                bitPos = 0;
                            }
                        }
                        x &= (uint)((1 << bitSize) - 1);
                        q[j] = x / (0.5f * ((1 << bitSize) - 1)) - 1;
                        sum += q[j] * q[j];
                    }
                }

                int lastComponent = (int)(flags & 3);
                q[lastComponent] = (float)Math.Sqrt(1 - sum);
                if ((flags & 4) != 0u)
                    q[lastComponent] = -q[lastComponent];
                data[i] = q;
            }

            return data;
        }
    }

    public class CompressedAnimationCurve : IYamlWriter
    {
        public string m_Path;
        public PackedIntVector m_Times;
        public PackedQuatVector m_Values;
        public PackedFloatVector m_Slopes;
        public int m_PreInfinity;
        public int m_PostInfinity;

        public CompressedAnimationCurve(UIReader reader)
        {
            m_Path = reader.ReadAlignedString();
            m_Times = new PackedIntVector(reader);
            m_Values = new PackedQuatVector(reader);
            m_Slopes = new PackedFloatVector(reader);
            m_PreInfinity = reader.Reader.ReadInt32();
            m_PostInfinity = reader.Reader.ReadInt32();
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(m_Path), m_Path);
        //    node.Add(nameof(m_Times), m_Times.ExportYAML(version));
        //    node.Add(nameof(m_Values), m_Values.ExportYAML(version));
        //    node.Add(nameof(m_Slopes), m_Slopes.ExportYAML(version));
        //    node.Add(nameof(m_PreInfinity), m_PreInfinity);
        //    node.Add(nameof(m_PostInfinity), m_PostInfinity);
        //    return node;
        //}
    }

    public class Vector3Curve : IYamlWriter
    {
        public AnimationCurve<Vector3> curve;
        public string path;

        public Vector3Curve(string path)
        {
            curve = new AnimationCurve<Vector3>();
            this.path = path;
        }

        public Vector3Curve(UIReader reader)
        {
            curve = new AnimationCurve<Vector3>(reader, reader.ReadVector3);
            path = reader.ReadAlignedString();
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    YAMLMappingNode node = new YAMLMappingNode();
        //    node.Add(nameof(curve), curve.ExportYAML(version));
        //    node.Add(nameof(path), path);
        //    return node;
        //}

        public override bool Equals(object obj)
        {
            if (obj is Vector3Curve vector3Curve)
            {
                return path == vector3Curve.path;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 577;
            unchecked
            {
                hash = 419 + hash * path.GetHashCode();
            }
            return hash;
        }
    }

    public class FloatCurve : IYamlWriter
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

        public FloatCurve(UIReader reader)
        {
            var version = reader.Version;

            curve = new AnimationCurve<float>(reader, reader.Reader.ReadSingle);
            attribute = reader.ReadAlignedString();
            path = reader.ReadAlignedString();
            classID = (ElementIDType)reader.Reader.ReadInt32();
            script = new PPtr<MonoScript>(reader);
            if (version.Major == 2022 && version.Minor >= 2) //2022.2 and up
            {
                flags = reader.Reader.ReadInt32();
            }
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    YAMLMappingNode node = new YAMLMappingNode();
        //    node.Add(nameof(curve), curve.ExportYAML(version));
        //    node.Add(nameof(attribute), attribute);
        //    node.Add(nameof(path), path);
        //    node.Add(nameof(classID), (int)classID);
        //    if (version[0] >= 2)
        //    {
        //        node.Add(nameof(script), script.ExportYAML(version));
        //    }
        //    node.Add(nameof(flags), flags);
        //    return node;
        //}

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

    public class PPtrKeyframe : IYamlWriter
    {
        public float time;
        public PPtr<UIObject> value;

        public PPtrKeyframe(float time, PPtr<UIObject> value)
        {
            this.time = time;
            this.value = value;
        }

        public PPtrKeyframe(UIReader reader)
        {
            time = reader.Reader.ReadSingle();
            value = new PPtr<UIObject>(reader);
        }
        //public YAMLNode ExportYAML(int[] version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(time), time);
        //    node.Add(nameof(value), value.ExportYAML(version));
        //    return node;
        //}
    }

    public class PPtrCurve : IYamlWriter
    {
        public List<PPtrKeyframe> curve;
        public string attribute;
        public string path;
        public int classID;
        public PPtr<MonoScript> script;
        public int flags;

        public PPtrCurve(string path, string attribute, ElementIDType classID, PPtr<MonoScript> script)
        {
            curve = new List<PPtrKeyframe>();
            this.attribute = attribute;
            this.path = path;
            this.classID = (int)classID;
            this.script = script;
            flags = 0;
        }

        public PPtrCurve(UIReader reader)
        {
            var version = reader.Version;

            int numCurves = reader.Reader.ReadInt32();
            curve = new List<PPtrKeyframe>();
            for (int i = 0; i < numCurves; i++)
            {
                curve.Add(new PPtrKeyframe(reader));
            }

            attribute = reader.ReadAlignedString();
            path = reader.ReadAlignedString();
            classID = reader.Reader.ReadInt32();
            script = new PPtr<MonoScript>(reader);
            if (version.Major == 2022 && version.Minor >= 2) //2022.2 and up
            {
                flags = reader.Reader.ReadInt32();
            }
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    YAMLMappingNode node = new YAMLMappingNode();
        //    node.Add(nameof(curve), curve.ExportYAML(version));
        //    node.Add(nameof(attribute), attribute);
        //    node.Add(nameof(path), path);
        //    node.Add(nameof(classID), (classID).ToString());
        //    node.Add(nameof(script), script.ExportYAML(version));
        //    node.Add(nameof(flags), flags);
        //    return node;
        //}

        public override bool Equals(object obj)
        {
            if (obj is PPtrCurve pptrCurve)
            {
                return this == pptrCurve;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 113;
            unchecked
            {
                hash = hash + 457 * attribute.GetHashCode();
                hash = hash * 433 + path.GetHashCode();
                hash = hash * 223 + classID.GetHashCode();
                hash = hash * 911 + script.GetHashCode();
                hash = hash * 342 + flags.GetHashCode();
            }
            return hash;
        }
    }

    public class AABB : IYamlWriter
    {
        public Vector3 m_Center;
        public Vector3 m_Extent;

        public AABB(UIReader reader)
        {
            m_Center = reader.ReadVector3();
            m_Extent = reader.ReadVector3();
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(m_Center), m_Center.ExportYAML(version));
        //    node.Add(nameof(m_Extent), m_Extent.ExportYAML(version));
        //    return node;
        //}
    }

    public class HandPose
    {
        public XForm<Vector3> m_GrabX;
        public float[] m_DoFArray;
        public float m_Override;
        public float m_CloseOpen;
        public float m_InOut;
        public float m_Grab;
        public HandPose() { }

        public HandPose(UIReader reader)
        {
            m_GrabX = reader.ReadXForm();
            m_DoFArray = reader.ReadArray(r => r.ReadSingle());
            m_Override = reader.Reader.ReadSingle();
            m_CloseOpen = reader.Reader.ReadSingle();
            m_InOut = reader.Reader.ReadSingle();
            m_Grab = reader.Reader.ReadSingle();
        }

        public static HandPose ParseGI(UIReader reader)
        {
            var handPose = new HandPose();
            handPose.m_GrabX = UIReader.Parse(reader.ReadXForm4());
            handPose.m_DoFArray = reader.ReadArray(20, r => r.ReadSingle());
            handPose.m_Override = reader.Reader.ReadSingle();
            handPose.m_CloseOpen = reader.Reader.ReadSingle();
            handPose.m_InOut = reader.Reader.ReadSingle();
            handPose.m_Grab = reader.Reader.ReadSingle();

            return handPose;
        }
    }

    public class HumanGoal
    {
        public XForm<Vector3> m_X;
        public float m_WeightT;
        public float m_WeightR;
        public Vector3 m_HintT;
        public float m_HintWeightT;
        public HumanGoal() { }

        public HumanGoal(UIReader reader)
        {
            var version = reader.Version;
            m_X = reader.ReadXForm();
            m_WeightT = reader.Reader.ReadSingle();
            m_WeightR = reader.Reader.ReadSingle();
            if (version.Major >= 5)//5.0 and up
            {
                m_HintT = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3() :
                    UIReader.Parse(reader.ReadVector4());//5.4 and up
                m_HintWeightT = reader.Reader.ReadSingle();
            }
        }

        public static HumanGoal ParseGI(UIReader reader)
        {
            var humanGoal = new HumanGoal();

            humanGoal.m_X = UIReader.Parse(reader.ReadXForm4());
            humanGoal.m_WeightT = reader.Reader.ReadSingle();
            humanGoal.m_WeightR = reader.Reader.ReadSingle();

            humanGoal.m_HintT = UIReader.Parse(reader.ReadVector4());
            humanGoal.m_HintWeightT = reader.Reader.ReadSingle();

            var m_HintR = UIReader.Parse(reader.ReadVector4());
            var m_HintWeightR = reader.Reader.ReadSingle();

            return humanGoal;
        }
    }

    public class HumanPose
    {
        public XForm<Vector3> m_RootX;
        public Vector3 m_LookAtPosition;
        public Vector4 m_LookAtWeight;
        public List<HumanGoal> m_GoalArray;
        public HandPose m_LeftHandPose;
        public HandPose m_RightHandPose;
        public float[] m_DoFArray;
        public Vector3[] m_TDoFArray;
        public HumanPose() { }

        public HumanPose(UIReader reader)
        {
            var version = reader.Version;
            m_RootX = reader.ReadXForm();
            m_LookAtPosition = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3() :
                UIReader.Parse(reader.ReadVector4());//5.4 and up
            m_LookAtWeight = reader.ReadVector4();

            int numGoals = reader.Reader.ReadInt32();
            m_GoalArray = new List<HumanGoal>();
            for (int i = 0; i < numGoals; i++)
            {
                m_GoalArray.Add(new HumanGoal(reader));
            }

            m_LeftHandPose = new HandPose(reader);
            m_RightHandPose = new HandPose(reader);

            m_DoFArray = reader.ReadArray(r => r.ReadSingle());

            if (version.GreaterThanOrEquals(5, 2))//5.2 and up
            {
                m_TDoFArray = reader.ReadVector3Array();
            }
        }

        public static HumanPose ParseGI(UIReader reader)
        {
            var version = reader.Version;
            var humanPose = new HumanPose();

            humanPose.m_RootX = UIReader.Parse(reader.ReadXForm4());
            humanPose.m_LookAtPosition = UIReader.Parse(reader.ReadVector4());
            humanPose.m_LookAtWeight = reader.ReadVector4();

            humanPose.m_GoalArray = new List<HumanGoal>();
            for (int i = 0; i < 4; i++)
            {
                humanPose.m_GoalArray.Add(HumanGoal.ParseGI(reader));
            }

            humanPose.m_LeftHandPose = HandPose.ParseGI(reader);
            humanPose.m_RightHandPose = HandPose.ParseGI(reader);

            humanPose.m_DoFArray = reader.ReadArray(0x37, r => r.ReadSingle());

            humanPose.m_TDoFArray = reader.ReadArray(0x15, _ => UIReader.Parse(reader.ReadVector4()));

            reader.Position += 4;

            return humanPose;
        }
    }

    public abstract class ACLClip
    {
        public virtual bool IsSet => false;
        public virtual uint CurveCount => 0;
        public abstract void Read(UIReader reader);
    }

    public class EmptyACLClip : ACLClip
    {
        public override void Read(UIReader reader) { }
    }

    public class MHYACLClip : ACLClip
    {
        public uint m_CurveCount;
        public uint m_ConstCurveCount;

        public byte[] m_ClipData;

        public override bool IsSet => m_ClipData is not null && m_ClipData.Length > 0;
        public override uint CurveCount => m_CurveCount;

        public MHYACLClip()
        {
            m_CurveCount = 0;
            m_ConstCurveCount = 0;
            m_ClipData = Array.Empty<byte>();
        }
        public override void Read(UIReader reader)
        {
            var byteCount = reader.Reader.ReadInt32();

            if (reader.IsSRGroup())
            {
                byteCount *= 4;
            }

            m_ClipData = reader.Reader.ReadBytes(byteCount);
            reader.Reader.AlignStream();

            m_CurveCount = reader.Reader.ReadUInt32();

            if (reader.IsSRGroup())
            {
                m_ConstCurveCount = reader.Reader.ReadUInt32();
            }
        }
    }

    public class AclTransformTrackIDToBindingCurveID
    {
        public uint rotationIDToBindingCurveID;
        public uint positionIDToBindingCurveID;
        public uint scaleIDToBindingCurveID;
        public AclTransformTrackIDToBindingCurveID(UIReader reader)
        {
            rotationIDToBindingCurveID = reader.Reader.ReadUInt32();
            positionIDToBindingCurveID = reader.Reader.ReadUInt32();
            scaleIDToBindingCurveID = reader.Reader.ReadUInt32();
        }
    }

    public class LnDACLClip : ACLClip
    {
        public uint m_CurveCount;
        public byte[] m_ClipData;

        public override bool IsSet => m_ClipData is not null && m_ClipData.Length > 0;
        public override uint CurveCount => m_CurveCount;
        public override void Read(UIReader reader)
        {
            m_CurveCount = reader.Reader.ReadUInt32();
            var compressedTransformTracksSize = reader.Reader.ReadUInt32();
            var compressedScalarTracksSize = reader.Reader.ReadUInt32();
            var aclTransformCount = reader.Reader.ReadUInt32();
            var aclScalarCount = reader.Reader.ReadUInt32();

            var compressedTransformTracksCount = reader.Reader.ReadInt32() * 0x10;
            var compressedTransformTracks = reader.Reader.ReadBytes(compressedTransformTracksCount);
            var compressedScalarTracksCount = reader.Reader.ReadInt32() * 0x10;
            var compressedScalarTracks = reader.Reader.ReadBytes(compressedScalarTracksCount);

            int numaclTransformTrackIDToBindingCurveID = reader.Reader.ReadInt32();
            var aclTransformTrackIDToBindingCurveID = new List<AclTransformTrackIDToBindingCurveID>();
            for (int i = 0; i < numaclTransformTrackIDToBindingCurveID; i++)
            {
                aclTransformTrackIDToBindingCurveID.Add(new AclTransformTrackIDToBindingCurveID(reader));
            }
            var aclScalarTrackIDToBindingCurveID = reader.ReadArray(r => r.ReadUInt32());
        }
    }

    public class GIACLClip : ACLClip
    {
        public uint m_CurveCount;
        public uint m_ConstCurveCount;

        public byte[] m_ClipData;
        public byte[] m_DatabaseData;

        public override bool IsSet => m_ClipData is not null && m_ClipData.Length > 0 && m_DatabaseData is not null && m_DatabaseData.Length > 0;
        public override uint CurveCount => m_CurveCount;

        public GIACLClip()
        {
            m_CurveCount = 0;
            m_ConstCurveCount = 0;
            m_ClipData = Array.Empty<byte>();
            m_DatabaseData = Array.Empty<byte>();
        }

        public override void Read(UIReader reader)
        {
            var aclTracksCount = (int)reader.Reader.ReadUInt64();
            var aclTracksOffset = reader.Position + reader.Reader.ReadInt64();
            var aclTracksCurveCount = reader.Reader.ReadUInt32();
            if (aclTracksOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = aclTracksOffset;

            var tracksBytes = reader.Reader.ReadBytes(aclTracksCount);
            reader.Reader.AlignStream();

            using var tracksMS = new MemoryStream();
            tracksMS.Write(tracksBytes);
            //tracksMS.AlignStream();
            m_CurveCount = aclTracksCurveCount;
            m_ClipData = tracksMS.ToArray();

            reader.Position = pos;

            var aclDatabaseCount = reader.Reader.ReadInt32();
            var aclDatabaseOffset = reader.Position + reader.Reader.ReadInt64();
            var aclDatabaseCurveCount = (uint)reader.Reader.ReadUInt64();
            if (aclDatabaseOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            pos = reader.Position;
            reader.Position = aclDatabaseOffset;

            var databaseBytes = reader.Reader.ReadBytes(aclDatabaseCount);
            reader.Reader.AlignStream();

            using var databaseMS = new MemoryStream();
            databaseMS.Write(databaseBytes);
            //databaseMS.AlignStream();

            m_ConstCurveCount = aclDatabaseCurveCount;
            m_DatabaseData = databaseMS.ToArray();

            reader.Position = pos;
        }
    }

    public class StreamedClip
    {
        public uint[] data;
        public uint curveCount;
        public StreamedClip() { }

        public StreamedClip(UIReader reader)
        {
            data = reader.ReadArray(r => r.ReadUInt32());
            curveCount = reader.Reader.ReadUInt32();
        }
        public static StreamedClip ParseGI(UIReader reader)
        {
            var streamedClipCount = (int)reader.Reader.ReadUInt64();
            var streamedClipOffset = reader.Position + reader.Reader.ReadInt64();
            var streamedClipCurveCount = (uint)reader.Reader.ReadUInt64();
            if (streamedClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = streamedClipOffset;

            var streamedClip = new StreamedClip()
            {
                data = reader.ReadArray(streamedClipCount, r => r.ReadUInt32()),
                curveCount = streamedClipCurveCount
            };

            reader.Position = pos;

            return streamedClip;
        }

        public class StreamedCurveKey
        {
            public int index;
            public float[] coeff;

            public float value;
            public float outSlope;
            public float inSlope;

            public StreamedCurveKey(EndianReader reader)
            {
                index = reader.ReadInt32();
                coeff = reader.ReadArray(4, r => r.ReadSingle());

                outSlope = coeff[2];
                value = coeff[3];
            }

            public float CalculateNextInSlope(float dx, StreamedCurveKey rhs)
            {
                //Stepped
                if (coeff[0] == 0f && coeff[1] == 0f && coeff[2] == 0f)
                {
                    return float.PositiveInfinity;
                }

                dx = Math.Max(dx, 0.0001f);
                var dy = rhs.value - value;
                var length = 1.0f / (dx * dx);
                var d1 = outSlope * dx;
                var d2 = dy + dy + dy - d1 - d1 - coeff[1] / length;
                return d2 / dx;
            }
        }

        public class StreamedFrame
        {
            public float time;
            public List<StreamedCurveKey> keyList;

            public StreamedFrame(EndianReader reader)
            {
                time = reader.ReadSingle();

                int numKeys = reader.ReadInt32();
                keyList = new List<StreamedCurveKey>();
                for (int i = 0; i < numKeys; i++)
                {
                    keyList.Add(new StreamedCurveKey(reader));
                }
            }
        }

        public List<StreamedFrame> ReadData()
        {
            var frameList = new List<StreamedFrame>();
            var buffer = new byte[data.Length * 4];
            Buffer.BlockCopy(data, 0, buffer, 0, buffer.Length);
            using (var reader = new EndianReader(new MemoryStream(buffer), EndianType.LittleEndian))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    frameList.Add(new StreamedFrame(reader));
                }
            }

            for (int frameIndex = 2; frameIndex < frameList.Count - 1; frameIndex++)
            {
                var frame = frameList[frameIndex];
                foreach (var curveKey in frame.keyList)
                {
                    for (int i = frameIndex - 1; i >= 0; i--)
                    {
                        var preFrame = frameList[i];
                        var preCurveKey = preFrame.keyList.FirstOrDefault(x => x.index == curveKey.index);
                        if (preCurveKey != null)
                        {
                            curveKey.inSlope = preCurveKey.CalculateNextInSlope(frame.time - preFrame.time, curveKey);
                            break;
                        }
                    }
                }
            }
            return frameList;
        }
    }

    public class DenseClip
    {
        public int m_FrameCount;
        public uint m_CurveCount;
        public float m_SampleRate;
        public float m_BeginTime;
        public float[] m_SampleArray;
        public DenseClip() { }

        public DenseClip(UIReader reader)
        {
            m_FrameCount = reader.Reader.ReadInt32();
            m_CurveCount = reader.Reader.ReadUInt32();
            m_SampleRate = reader.Reader.ReadSingle();
            m_BeginTime = reader.Reader.ReadSingle();
            m_SampleArray = reader.Reader.ReadArray(r => r.ReadSingle());
        }
        public static DenseClip ParseGI(UIReader reader)
        {
            var denseClip = new DenseClip();

            denseClip.m_FrameCount = reader.Reader.ReadInt32();
            denseClip.m_CurveCount = reader.Reader.ReadUInt32();
            denseClip.m_SampleRate = reader.Reader.ReadSingle();
            denseClip.m_BeginTime = reader.Reader.ReadSingle();

            var denseClipCount = (int)reader.Reader.ReadUInt64();
            var denseClipOffset = reader.Position + reader.Reader.ReadInt64();
            if (denseClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = denseClipOffset;

            denseClip.m_SampleArray = reader.ReadArray(denseClipCount, r => r.ReadSingle());

            reader.Position = pos;

            return denseClip;
        }
    }
    public class ACLDenseClip : DenseClip
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

        public ACLDenseClip(UIReader reader) : base(reader)
        {
            m_ACLType = reader.Reader.ReadInt32();
            if (reader.IsArknightsEndfield())
            {
                m_ACLArray = reader.ReadArray(r => r.ReadByte());
                reader.Reader.AlignStream();
                m_PositionFactor = reader.Reader.ReadSingle();
                m_EulerFactor = reader.Reader.ReadSingle();
                m_ScaleFactor = reader.Reader.ReadSingle();
                m_FloatFactor = reader.Reader.ReadSingle();
                m_nPositionCurves = reader.Reader.ReadUInt32();
                m_nRotationCurves = reader.Reader.ReadUInt32();
                m_nEulerCurves = reader.Reader.ReadUInt32();
                m_nScaleCurves = reader.Reader.ReadUInt32();
            }
            else if (reader.IsExAstris())
            {
                m_nPositionCurves = reader.Reader.ReadUInt32();
                m_nRotationCurves = reader.Reader.ReadUInt32();
                m_nEulerCurves = reader.Reader.ReadUInt32();
                m_nScaleCurves = reader.Reader.ReadUInt32();
                m_nGenericCurves = reader.Reader.ReadUInt32();
                m_PositionFactor = reader.Reader.ReadSingle();
                m_EulerFactor = reader.Reader.ReadSingle();
                m_ScaleFactor = reader.Reader.ReadSingle();
                m_FloatFactor = reader.Reader.ReadSingle();
                m_ACLArray = reader.Reader.ReadArray(r => r.ReadByte());
                reader.Reader.AlignStream();
            }
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

    public class ConstantClip
    {
        public float[] data;
        public ConstantClip() { }

        public ConstantClip(UIReader reader)
        {
            data = reader.ReadArray(r => r.ReadSingle());
        }
        public static ConstantClip ParseGI(UIReader reader)
        {
            var constantClipCount = (int)reader.Reader.ReadUInt64();
            var constantClipOffset = reader.Position + reader.Reader.ReadInt64();
            if (constantClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = constantClipOffset;

            var constantClip = new ConstantClip();
            constantClip.data = reader.ReadArray(constantClipCount, r => r.ReadSingle());

            reader.Position = pos;

            return constantClip;
        }
    }

    public class ValueConstant
    {
        public uint m_ID;
        public uint m_TypeID;
        public uint m_Type;
        public uint m_Index;

        public ValueConstant(UIReader reader)
        {
            var version = reader.Version;
            m_ID = reader.Reader.ReadUInt32();
            if (version.LessThan(5, 5))//5.5 down
            {
                m_TypeID = reader.Reader.ReadUInt32();
            }
            m_Type = reader.Reader.ReadUInt32();
            m_Index = reader.Reader.ReadUInt32();
        }
    }

    public class ValueArrayConstant
    {
        public List<ValueConstant> m_ValueArray;

        public ValueArrayConstant(UIReader reader)
        {
            int numVals = reader.Reader.ReadInt32();
            m_ValueArray = new List<ValueConstant>();
            for (int i = 0; i < numVals; i++)
            {
                m_ValueArray.Add(new ValueConstant(reader));
            }
        }
    }

    public class Clip
    {
        public ACLClip m_ACLClip = new EmptyACLClip();
        public StreamedClip m_StreamedClip;
        public DenseClip m_DenseClip;
        public ConstantClip m_ConstantClip;
        public ValueArrayConstant m_Binding;
        public Clip() { }

        public Clip(UIReader reader)
        {
            var version = reader.Version;
            m_StreamedClip = new StreamedClip(reader);
            if (reader.IsArknightsEndfield() || reader.IsExAstris())
            {
                m_DenseClip = new ACLDenseClip(reader);
            }
            else
            {
                m_DenseClip = new DenseClip(reader);
            }
            if (reader.IsSRGroup())
            {
                m_ACLClip = new MHYACLClip();
                m_ACLClip.Read(reader);
            }
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_ConstantClip = new ConstantClip(reader);
            }
            if (reader.IsGIGroup() || reader.IsBH3Group() || reader.IsZZZCB1())
            {
                m_ACLClip = new MHYACLClip();
                m_ACLClip.Read(reader);
            }
            if (reader.IsLoveAndDeepspace())
            {
                m_ACLClip = new LnDACLClip();
                m_ACLClip.Read(reader);
            }
            if (version.LessThan(2018, 3)) //2018.3 down
            {
                m_Binding = new ValueArrayConstant(reader);
            }
        }
        public static Clip ParseGI(UIReader reader)
        {
            var clipOffset = reader.Position + reader.Reader.ReadInt64();
            if (clipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = clipOffset;

            var clip = new Clip();
            clip.m_StreamedClip = StreamedClip.ParseGI(reader);
            clip.m_DenseClip = DenseClip.ParseGI(reader);
            clip.m_ConstantClip = ConstantClip.ParseGI(reader);
            clip.m_ACLClip = new GIACLClip();
            clip.m_ACLClip.Read(reader);

            reader.Position = pos;

            return clip;
        }

        public AnimationClipBindingConstant ConvertValueArrayToGenericBinding()
        {
            var bindings = new AnimationClipBindingConstant();
            var genericBindings = new List<GenericBinding>();
            var values = m_Binding;
            for (int i = 0; i < values.m_ValueArray.Count;)
            {
                var curveID = values.m_ValueArray[i].m_ID;
                var curveTypeID = values.m_ValueArray[i].m_TypeID;
                var binding = new GenericBinding();
                genericBindings.Add(binding);
                if (curveTypeID == 4174552735) //CRC(PositionX))
                {
                    binding.path = curveID;
                    binding.attribute = 1; //kBindTransformPosition
                    binding.typeID = ElementIDType.Transform;
                    i += 3;
                }
                else if (curveTypeID == 2211994246) //CRC(QuaternionX))
                {
                    binding.path = curveID;
                    binding.attribute = 2; //kBindTransformRotation
                    binding.typeID = ElementIDType.Transform;
                    i += 4;
                }
                else if (curveTypeID == 1512518241) //CRC(ScaleX))
                {
                    binding.path = curveID;
                    binding.attribute = 3; //kBindTransformScale
                    binding.typeID = ElementIDType.Transform;
                    i += 3;
                }
                else
                {
                    binding.typeID = ElementIDType.Animator;
                    binding.path = 0;
                    binding.attribute = curveID;
                    i++;
                }
            }
            bindings.genericBindings = genericBindings;
            return bindings;
        }
    }

    public class ValueDelta
    {
        public float m_Start;
        public float m_Stop;

        public ValueDelta(UIReader reader)
        {
            m_Start = reader.Reader.ReadSingle();
            m_Stop = reader.Reader.ReadSingle();
        }
    }

    public class ClipMuscleConstant : IYamlWriter
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
            if (reader.IsLoveAndDeepspace())
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
            m_StartTime = reader.Reader.ReadSingle();
            m_StopTime = reader.Reader.ReadSingle();
            m_OrientationOffsetY = reader.Reader.ReadSingle();
            m_Level = reader.Reader.ReadSingle();
            m_CycleOffset = reader.Reader.ReadSingle();
            m_AverageAngularSpeed = reader.Reader.ReadSingle();

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
            int numDeltas = reader.Reader.ReadInt32();
            m_ValueArrayDelta = new List<ValueDelta>();
            for (int i = 0; i < numDeltas; i++)
            {
                m_ValueArrayDelta.Add(new ValueDelta(reader));
            }
            if (version.GreaterThanOrEquals(5, 3))//5.3 and up
            {
                m_ValueArrayReferencePose = reader.ReadArray(r => r.ReadSingle());
            }

            m_Mirror = reader.Reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_LoopTime = reader.Reader.ReadBoolean();
            }
            m_LoopBlend = reader.Reader.ReadBoolean();
            m_LoopBlendOrientation = reader.Reader.ReadBoolean();
            m_LoopBlendPositionY = reader.Reader.ReadBoolean();
            m_LoopBlendPositionXZ = reader.Reader.ReadBoolean();
            if (version.GreaterThanOrEquals(5, 5))//5.5 and up
            {
                m_StartAtOrigin = reader.Reader.ReadBoolean();
            }
            m_KeepOriginalOrientation = reader.Reader.ReadBoolean();
            m_KeepOriginalPositionY = reader.Reader.ReadBoolean();
            m_KeepOriginalPositionXZ = reader.Reader.ReadBoolean();
            m_HeightFromFeet = reader.Reader.ReadBoolean();
            reader.Reader.AlignStream();
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

            clipMuscleConstant.m_StartTime = reader.Reader.ReadSingle();
            clipMuscleConstant.m_StopTime = reader.Reader.ReadSingle();
            clipMuscleConstant.m_OrientationOffsetY = reader.Reader.ReadSingle();
            clipMuscleConstant.m_Level = reader.Reader.ReadSingle();
            clipMuscleConstant.m_CycleOffset = reader.Reader.ReadSingle();
            clipMuscleConstant.m_AverageAngularSpeed = reader.Reader.ReadSingle();

            clipMuscleConstant.m_IndexArray = reader.ReadArray(0xC8, r => (int)r.ReadInt16());

            var valueArrayDeltaCount = (int)reader.Reader.ReadUInt64();
            var valueArrayDeltaOffset = reader.Position + reader.Reader.ReadInt64();

            if (valueArrayDeltaOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var valueArrayReferencePoseCount = (int)reader.Reader.ReadUInt64();
            var valueArrayReferencePoseOffset = reader.Position + reader.Reader.ReadInt64();

            if (valueArrayReferencePoseOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            clipMuscleConstant.m_Mirror = reader.Reader.ReadBoolean();
            clipMuscleConstant.m_LoopTime = reader.Reader.ReadBoolean();
            clipMuscleConstant.m_LoopBlend = reader.Reader.ReadBoolean();
            clipMuscleConstant.m_LoopBlendOrientation = reader.Reader.ReadBoolean();
            clipMuscleConstant.m_LoopBlendPositionY = reader.Reader.ReadBoolean();
            clipMuscleConstant.m_LoopBlendPositionXZ = reader.Reader.ReadBoolean();
            clipMuscleConstant.m_StartAtOrigin = reader.Reader.ReadBoolean();
            clipMuscleConstant.m_KeepOriginalOrientation = reader.Reader.ReadBoolean();
            clipMuscleConstant.m_KeepOriginalPositionY = reader.Reader.ReadBoolean();
            clipMuscleConstant.m_KeepOriginalPositionXZ = reader.Reader.ReadBoolean();
            clipMuscleConstant.m_HeightFromFeet = reader.Reader.ReadBoolean();
            reader.Reader.AlignStream();

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

    public class GenericBinding : IYamlWriter
    {
        public UnityVersion version;
        public uint path;
        public uint attribute;
        public PPtr<UIObject> script;
        public ElementIDType typeID;
        public byte customType;
        public byte isPPtrCurve;
        public byte isIntCurve;

        public GenericBinding() { }

        public GenericBinding(UIReader reader)
        {
            version = reader.Version;
            path = reader.Reader.ReadUInt32();
            attribute = reader.Reader.ReadUInt32();
            script = new PPtr<UIObject>(reader);
            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                typeID = (ElementIDType)reader.Reader.ReadInt32();
            }
            else
            {
                typeID = (ElementIDType)reader.Reader.ReadUInt16();
            }
            customType = reader.Reader.ReadByte();
            isPPtrCurve = reader.Reader.ReadByte();
            if (version.GreaterThanOrEquals(2022, 1)) //2022.1 and up
            {
                isIntCurve = reader.Reader.ReadByte();
            }
            reader.Reader.AlignStream();
        }

        //public YAMLNode ExportYAML(int[] version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(path), path);
        //    node.Add(nameof(attribute), attribute);
        //    node.Add(nameof(script), script.ExportYAML(version));
        //    node.Add("classID", ((int)typeID).ToString());
        //    node.Add(nameof(customType), customType);
        //    node.Add(nameof(isPPtrCurve), isPPtrCurve);
        //    return node;
        //}
    }

    public class AnimationClipBindingConstant : IYamlWriter
    {
        public List<GenericBinding> genericBindings;
        public List<PPtr<UIObject>> pptrCurveMapping;

        public AnimationClipBindingConstant() { }

        public AnimationClipBindingConstant(UIReader reader)
        {
            int numBindings = reader.Reader.ReadInt32();
            genericBindings = new List<GenericBinding>();
            for (int i = 0; i < numBindings; i++)
            {
                genericBindings.Add(new GenericBinding(reader));
            }

            int numMappings = reader.Reader.ReadInt32();
            pptrCurveMapping = new List<PPtr<UIObject>>();
            for (int i = 0; i < numMappings; i++)
            {
                pptrCurveMapping.Add(new PPtr<UIObject>(reader));
            }
        }

        //public YAMLNode ExportYAML(int[] version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(genericBindings), genericBindings.ExportYAML(version));
        //    node.Add(nameof(pptrCurveMapping), pptrCurveMapping.ExportYAML(version));
        //    return node;
        //}

        public GenericBinding FindBinding(int index)
        {
            int curves = 0;
            foreach (var b in genericBindings)
            {
                if (b.typeID == ElementIDType.Transform)
                {
                    switch (b.attribute)
                    {
                        case 1: //kBindTransformPosition
                        case 3: //kBindTransformScale
                        case 4: //kBindTransformEuler
                            curves += 3;
                            break;
                        case 2: //kBindTransformRotation
                            curves += 4;
                            break;
                        default:
                            curves += 1;
                            break;
                    }
                }
                else
                {
                    curves += 1;
                }
                if (curves > index)
                {
                    return b;
                }
            }

            return null;
        }
    }

    public class AnimationEvent : IYamlWriter
    {
        public float time;
        public string functionName;
        public string data;
        public PPtr<UIObject> objectReferenceParameter;
        public float floatParameter;
        public int intParameter;
        public int messageOptions;

        public AnimationEvent(UIReader reader)
        {
            var version = reader.Version;

            time = reader.Reader.ReadSingle();
            functionName = reader.ReadAlignedString();
            data = reader.ReadAlignedString();
            objectReferenceParameter = new PPtr<UIObject>(reader);
            floatParameter = reader.Reader.ReadSingle();
            if (version.Major >= 3) //3 and up
            {
                intParameter = reader.Reader.ReadInt32();
            }
            messageOptions = reader.Reader.ReadInt32();
        }

        //public YAMLNode ExportYAML(int[] version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(time), time);
        //    node.Add(nameof(functionName), functionName);
        //    node.Add(nameof(data), data);
        //    node.Add(nameof(objectReferenceParameter), objectReferenceParameter.ExportYAML(version));
        //    node.Add(nameof(floatParameter), floatParameter);
        //    node.Add(nameof(intParameter), intParameter);
        //    node.Add(nameof(messageOptions), messageOptions);
        //    return node;
        //}
    }

    public enum AnimationType
    {
        Legacy = 1,
        Generic = 2,
        Humanoid = 3
    };

    public sealed class AnimationClip : NamedObject, IFileWriter
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

        public AnimationClip(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            if (version.Major >= 5)//5.0 and up
            {
                m_Legacy = reader.Reader.ReadBoolean();
            }
            else if (version.Major >= 4)//4.0 and up
            {
                m_AnimationType = (AnimationType)reader.Reader.ReadInt32();
                if (m_AnimationType == AnimationType.Legacy)
                    m_Legacy = true;
            }
            else
            {
                m_Legacy = true;
            }
            if (reader.IsLoveAndDeepspace())
            {
                reader.Reader.AlignStream();
                var m_aclTransformCache = reader.ReadArray(r => r.ReadByte());
                var m_aclScalarCache = reader.ReadArray(r => r.ReadByte());
                int numaclTransformTrackId2CurveId = reader.Reader.ReadInt32();
                var m_aclTransformTrackId2CurveId = new List<AclTransformTrackIDToBindingCurveID>();
                for (int i = 0; i < numaclTransformTrackId2CurveId; i++)
                {
                    m_aclTransformTrackId2CurveId.Add(new AclTransformTrackIDToBindingCurveID(reader));
                }
                var m_aclScalarTrackId2CurveId = reader.ReadArray(r => r.ReadUInt32());
            }
            m_Compressed = reader.Reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 3))//4.3 and up
            {
                m_UseHighQualityCurve = reader.Reader.ReadBoolean();
            }
            reader.Reader.AlignStream();
            int numRCurves = reader.Reader.ReadInt32();
            m_RotationCurves = new List<QuaternionCurve>();
            for (int i = 0; i < numRCurves; i++)
            {
                m_RotationCurves.Add(new QuaternionCurve(reader));
            }

            int numCRCurves = reader.Reader.ReadInt32();
            m_CompressedRotationCurves = new List<CompressedAnimationCurve>();
            for (int i = 0; i < numCRCurves; i++)
            {
                m_CompressedRotationCurves.Add(new CompressedAnimationCurve(reader));
            }

            if (reader.IsExAstris())
            {
                var m_aclType = reader.Reader.ReadInt32();
            }

            if (version.GreaterThanOrEquals(5, 3))//5.3 and up
            {
                int numEulerCurves = reader.Reader.ReadInt32();
                m_EulerCurves = new List<Vector3Curve>();
                for (int i = 0; i < numEulerCurves; i++)
                {
                    m_EulerCurves.Add(new Vector3Curve(reader));
                }
            }

            int numPCurves = reader.Reader.ReadInt32();
            m_PositionCurves = new List<Vector3Curve>();
            for (int i = 0; i < numPCurves; i++)
            {
                m_PositionCurves.Add(new Vector3Curve(reader));
            }

            int numSCurves = reader.Reader.ReadInt32();
            m_ScaleCurves = new List<Vector3Curve>();
            for (int i = 0; i < numSCurves; i++)
            {
                m_ScaleCurves.Add(new Vector3Curve(reader));
            }

            int numFCurves = reader.Reader.ReadInt32();
            m_FloatCurves = new List<FloatCurve>();
            for (int i = 0; i < numFCurves; i++)
            {
                m_FloatCurves.Add(new FloatCurve(reader));
            }

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                int numPtrCurves = reader.Reader.ReadInt32();
                m_PPtrCurves = new List<PPtrCurve>();
                for (int i = 0; i < numPtrCurves; i++)
                {
                    m_PPtrCurves.Add(new PPtrCurve(reader));
                }
            }

            m_SampleRate = reader.Reader.ReadSingle();
            m_WrapMode = reader.Reader.ReadInt32();
            if (reader.IsArknightsEndfield())
            {
                var m_aclType = reader.Reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(3, 4)) //3.4 and up
            {
                m_Bounds = new AABB(reader);
            }
            if (version.Major >= 4)//4.0 and up
            {
                if (reader.IsGI())
                {
                    var muscleClipSize = reader.Reader.ReadInt32();
                    if (muscleClipSize < 0)
                    {
                        hasStreamingInfo = true;
                        m_MuscleClipSize = reader.Reader.ReadUInt32();
                        var pos = reader.Position;
                        m_MuscleClip = ClipMuscleConstant.ParseGI(reader);
                        reader.Position = pos + m_MuscleClipSize;
                    }
                    else if (muscleClipSize > 0)
                    {
                        m_MuscleClipSize = (uint)muscleClipSize;
                        m_MuscleClip = new ClipMuscleConstant(reader);
                    }
                }
                else
                {
                    m_MuscleClipSize = reader.Reader.ReadUInt32();
                    m_MuscleClip = new ClipMuscleConstant(reader);
                }
            }
            if (reader.IsSRGroup())
            {
                var m_AclClipData = reader.ReadArray(r => r.ReadByte());
                var aclBindingsCount = reader.Reader.ReadInt32();
                var m_AclBindings = new List<GenericBinding>();
                for (int i = 0; i < aclBindingsCount; i++)
                {
                    m_AclBindings.Add(new GenericBinding(reader));
                }
                var m_AclRange = new KeyValuePair<float, float>(
                    reader.Reader.ReadSingle(), reader.Reader.ReadSingle());
            }
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_ClipBindingConstant = new AnimationClipBindingConstant(reader);
            }
            if (version.GreaterThanOrEquals(2018, 3)) //2018.3 and up
            {
                var m_HasGenericRootTransform = reader.Reader.ReadBoolean();
                var m_HasMotionFloatCurves = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();
            }
            int numEvents = reader.Reader.ReadInt32();
            m_Events = new List<AnimationEvent>();
            for (int i = 0; i < numEvents; i++)
            {
                m_Events.Add(new AnimationEvent(reader));
            }
            if (version.Major >= 2017) //2017 and up
            {
                reader.Reader.AlignStream();
            }
            if (hasStreamingInfo)
            {
                m_StreamData = new StreamingInfo(reader);
                if (!string.IsNullOrEmpty(m_StreamData?.path))
                {
                    var aclClip = m_MuscleClip.m_Clip.m_ACLClip as GIACLClip;

                    //var resourceReader = new ResourceReader(m_StreamData.path, 
                    //    assetsFile, m_StreamData.offset, m_StreamData.size);
                    var res = new PartialStream(reader.Reader.BaseStream, m_StreamData.offset, m_StreamData.size);
                    using var ms = new MemoryStream();
                    ms.Write(aclClip.m_DatabaseData);

                    //ms.Write(res.GetData());
                    res.CopyTo(ms);
                    //ms.AlignStream();

                    aclClip.m_DatabaseData = ms.ToArray();
                }
            }
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            throw new NotImplementedException();
        }
    }
}
