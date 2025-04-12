using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class Material(UIReader reader) : NamedObject(reader)
    {
        public PPtr<Shader> m_Shader;
        public UnityPropertySheet m_SavedProperties;

        public void ReadBase(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var version = reader.Get<UnityVersion>();
            m_Shader = new PPtr<Shader>(reader);

            if (version.Major == 4 && version.Minor >= 1) //4.x
            {
                var m_ShaderKeywords = reader.ReadArray(r => r.ReadAlignedString());
            }

            if (version.GreaterThanOrEquals(2021, 3)) //2021.3 and up
            {
                var m_ValidKeywords = reader.ReadArray(r => r.ReadAlignedString());
                var m_InvalidKeywords = reader.ReadArray(r => r.ReadAlignedString());
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
        }

        public override void Read(IBundleBinaryReader reader)
        {
            ReadBase(reader);
            var version = reader.Get<UnityVersion>();

            if (version.GreaterThanOrEquals(5, 1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }


            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadArray(r => r.ReadAlignedString());
            }

            m_SavedProperties = new UnityPropertySheet(reader);

            //vector m_BuildTextureStacks 2020 and up
        }

        public override void Associated(IDependencyBuilder? builder)
        {
            base.Associated(builder);
            builder?.AddDependencyEntry(_reader.FullPath, FileID, m_Shader.m_PathID);
        }
    }
}
