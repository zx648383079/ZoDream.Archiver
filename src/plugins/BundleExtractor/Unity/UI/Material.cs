using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class Material : NamedObject
    {
        public PPtr<Shader> m_Shader;
        public UnityPropertySheet m_SavedProperties;

        public Material(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            m_Shader = new PPtr<Shader>(reader);

            if (version.Major == 4 && version.Minor >= 1) //4.x
            {
                var m_ShaderKeywords = reader.ReadArray(r => r.ReadString());
            }

            if (version.GreaterThanOrEquals(2021, 3)) //2021.3 and up
            {
                var m_ValidKeywords = reader.ReadArray(r => r.ReadString());
                var m_InvalidKeywords = reader.ReadArray(r => r.ReadString());
            }
            else if (version.GreaterThanOrEquals(5)) //5.0 ~ 2021.2
            {
                var m_ShaderKeywords = reader.ReadAlignedString();
            }

            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                var m_LightmapFlags = reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var m_EnableInstancingVariants = reader.ReadBoolean();
                //var m_DoubleSidedGI = a_Stream.ReadBoolean(); //2017 and up
                reader.AlignStream();
            }
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                var m_CustomRenderQueue = reader.ReadInt32();
            }

            if (reader.IsLoveAndDeepSpace() || reader.IsShiningNikki() && version.Major >= 2019)
            {
                var m_MaterialType = reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(5, 1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }

            if (reader.IsNaraka())
            {
                var value = reader.ReadInt32();
            }

            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadArray(r => r.ReadString());
            }

            m_SavedProperties = new UnityPropertySheet(reader);

            //vector m_BuildTextureStacks 2020 and up
        }
    }
}
