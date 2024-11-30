using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedShaderState
    {
        public string m_Name;
        public List<SerializedShaderRTBlendState> rtBlend;
        public bool rtSeparateBlend;
        public SerializedShaderFloatValue zClip;
        public SerializedShaderFloatValue zTest;
        public SerializedShaderFloatValue zWrite;
        public SerializedShaderFloatValue culling;
        public SerializedShaderFloatValue conservative;
        public SerializedShaderFloatValue offsetFactor;
        public SerializedShaderFloatValue offsetUnits;
        public SerializedShaderFloatValue alphaToMask;
        public SerializedStencilOp stencilOp;
        public SerializedStencilOp stencilOpFront;
        public SerializedStencilOp stencilOpBack;
        public SerializedShaderFloatValue stencilReadMask;
        public SerializedShaderFloatValue stencilWriteMask;
        public SerializedShaderFloatValue stencilRef;
        public SerializedShaderFloatValue fogStart;
        public SerializedShaderFloatValue fogEnd;
        public SerializedShaderFloatValue fogDensity;
        public SerializedShaderVectorValue fogColor;
        public FogMode fogMode;
        public int gpuProgramID;
        public SerializedTagMap m_Tags;
        public int m_LOD;
        public bool lighting;

        public SerializedShaderState(UIReader reader)
        {
            var version = reader.Version;

            m_Name = reader.ReadAlignedString();
            rtBlend = new List<SerializedShaderRTBlendState>();
            for (int i = 0; i < 8; i++)
            {
                rtBlend.Add(new SerializedShaderRTBlendState(reader));
            }
            rtSeparateBlend = reader.ReadBoolean();
            reader.AlignStream();
            if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
            {
                zClip = new SerializedShaderFloatValue(reader);
            }
            zTest = new SerializedShaderFloatValue(reader);
            zWrite = new SerializedShaderFloatValue(reader);
            culling = new SerializedShaderFloatValue(reader);
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                conservative = new SerializedShaderFloatValue(reader);
            }
            offsetFactor = new SerializedShaderFloatValue(reader);
            offsetUnits = new SerializedShaderFloatValue(reader);
            alphaToMask = new SerializedShaderFloatValue(reader);
            stencilOp = new SerializedStencilOp(reader);
            stencilOpFront = new SerializedStencilOp(reader);
            stencilOpBack = new SerializedStencilOp(reader);
            stencilReadMask = new SerializedShaderFloatValue(reader);
            stencilWriteMask = new SerializedShaderFloatValue(reader);
            stencilRef = new SerializedShaderFloatValue(reader);
            fogStart = new SerializedShaderFloatValue(reader);
            fogEnd = new SerializedShaderFloatValue(reader);
            fogDensity = new SerializedShaderFloatValue(reader);
            fogColor = new SerializedShaderVectorValue(reader);
            fogMode = (FogMode)reader.ReadInt32();
            gpuProgramID = reader.ReadInt32();
            m_Tags = new SerializedTagMap(reader);
            m_LOD = reader.ReadInt32();
            if (reader.IsLoveAndDeepSpace())
            {
                int numOverrideKeywordAndStage = reader.ReadInt32();
                var m_OverrideKeywordAndStage = new List<KeyValuePair<string, uint>>();
                for (int i = 0; i < numOverrideKeywordAndStage; i++)
                {
                    m_OverrideKeywordAndStage.Add(new KeyValuePair<string, uint>(reader.ReadAlignedString(), reader.ReadUInt32()));
                }
            }
            lighting = reader.ReadBoolean();
            reader.AlignStream();
        }
    }

}
