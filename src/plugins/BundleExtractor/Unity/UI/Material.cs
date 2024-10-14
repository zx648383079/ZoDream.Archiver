using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class UnityTexEnv
    {
        public PPtr<Texture> m_Texture;
        public Vector2 m_Scale;
        public Vector2 m_Offset;

        public UnityTexEnv(UIReader reader)
        {
            m_Texture = new PPtr<Texture>(reader);
            m_Scale = reader.ReadVector2();
            m_Offset = reader.ReadVector2();
            if (reader.IsArknightsEndfield())
            {
                var m_UVSetIndex = reader.Reader.ReadInt32();
            }
        }
    }

    public class UnityPropertySheet
    {
        public List<KeyValuePair<string, UnityTexEnv>> m_TexEnvs;
        public List<KeyValuePair<string, int>> m_Ints;
        public List<KeyValuePair<string, float>> m_Floats;
        public List<KeyValuePair<string, Vector4>> m_Colors;

        public UnityPropertySheet(UIReader reader)
        {
            var version = reader.Version;

            int m_TexEnvsSize = reader.Reader.ReadInt32();
            m_TexEnvs = new List<KeyValuePair<string, UnityTexEnv>>();
            for (int i = 0; i < m_TexEnvsSize; i++)
            {
                m_TexEnvs.Add(new(reader.ReadAlignedString(), new UnityTexEnv(reader)));
            }

            if (version.GreaterThanOrEquals(2021, 1)) //2021.1 and up
            {
                int m_IntsSize = reader.Reader.ReadInt32();
                m_Ints = new List<KeyValuePair<string, int>>();
                for (int i = 0; i < m_IntsSize; i++)
                {
                    m_Ints.Add(new(reader.ReadAlignedString(), reader.Reader.ReadInt32()));
                }
            }

            int m_FloatsSize = reader.Reader.ReadInt32();
            m_Floats = new List<KeyValuePair<string, float>>();
            for (int i = 0; i < m_FloatsSize; i++)
            {
                m_Floats.Add(new(reader.ReadAlignedString(), reader.Reader.ReadSingle()));
            }

            int m_ColorsSize = reader.Reader.ReadInt32();
            m_Colors = new List<KeyValuePair<string, Vector4>>();
            for (int i = 0; i < m_ColorsSize; i++)
            {
                m_Colors.Add(new(reader.ReadAlignedString(), reader.ReadVector4()));
            }
        }
    }

    public sealed class Material : NamedObject
    {
        public PPtr<Shader> m_Shader;
        public UnityPropertySheet m_SavedProperties;

        public Material(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            m_Shader = new PPtr<Shader>(reader);

            if (version.Major == 4 && version.Minor >= 1) //4.x
            {
                var m_ShaderKeywords = reader.Reader.ReadArray(r => r.ReadString());
            }

            if (version.GreaterThanOrEquals(2021, 3)) //2021.3 and up
            {
                var m_ValidKeywords = reader.Reader.ReadArray(r => r.ReadString());
                var m_InvalidKeywords = reader.Reader.ReadArray(r => r.ReadString());
            }
            else if (version.GreaterThanOrEquals(5)) //5.0 ~ 2021.2
            {
                var m_ShaderKeywords = reader.ReadAlignedString();
            }

            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                var m_LightmapFlags = reader.Reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var m_EnableInstancingVariants = reader.Reader.ReadBoolean();
                //var m_DoubleSidedGI = a_Stream.ReadBoolean(); //2017 and up
                reader.Reader.AlignStream();
            }
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                var m_CustomRenderQueue = reader.Reader.ReadInt32();
            }

            if (reader.IsLoveAndDeepspace() || reader.IsShiningNikki() && version.Major >= 2019)
            {
                var m_MaterialType = reader.Reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(5, 1)) //5.1 and up
            {
                var stringTagMapSize = reader.Reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }

            if (reader.IsNaraka())
            {
                var value = reader.Reader.ReadInt32();
            }

            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.Reader.ReadArray(r => r.ReadString());
            }

            m_SavedProperties = new UnityPropertySheet(reader);

            //vector m_BuildTextureStacks 2020 and up
        }
    }
}
