using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Clip
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
            if (reader.IsLoveAndDeepSpace())
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
            var clipOffset = reader.Position + reader.ReadInt64();
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
}
