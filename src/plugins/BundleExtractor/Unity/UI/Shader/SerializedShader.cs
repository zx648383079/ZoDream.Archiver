using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedShader
    {
        public SerializedProperties m_PropInfo;
        public List<SerializedSubShader> m_SubShaders;
        public string[] m_KeywordNames;
        public byte[] m_KeywordFlags;
        public string m_Name;
        public string m_CustomEditorName;
        public string m_FallbackName;
        public List<SerializedShaderDependency> m_Dependencies;
        public List<SerializedCustomEditorForRenderPipeline> m_CustomEditorForRenderPipelines;
        public bool m_DisableNoSubshadersMessage;

        public SerializedShader(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            m_PropInfo = new SerializedProperties(reader);

            int numSubShaders = reader.ReadInt32();
            m_SubShaders = [];
            for (int i = 0; i < numSubShaders; i++)
            {
                m_SubShaders.Add(new SerializedSubShader(reader));
            }

            if (version.GreaterThanOrEquals(2021, 2)) //2021.2 and up
            {
                m_KeywordNames = reader.ReadArray(r => r.ReadAlignedString());
                m_KeywordFlags = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
            }

            m_Name = reader.ReadAlignedString();
            m_CustomEditorName = reader.ReadAlignedString();
            m_FallbackName = reader.ReadAlignedString();

            int numDependencies = reader.ReadInt32();
            m_Dependencies = [];
            for (int i = 0; i < numDependencies; i++)
            {
                m_Dependencies.Add(new SerializedShaderDependency(reader));
            }

            if (version.GreaterThanOrEquals(2021, 1)) //2021.1 and up
            {
                int m_CustomEditorForRenderPipelinesSize = reader.ReadInt32();
                m_CustomEditorForRenderPipelines = [];
                for (int i = 0; i < m_CustomEditorForRenderPipelinesSize; i++)
                {
                    m_CustomEditorForRenderPipelines.Add(new SerializedCustomEditorForRenderPipeline(reader));
                }
            }

            m_DisableNoSubshadersMessage = reader.ReadBoolean();
            reader.AlignStream();
        }
    }

}
