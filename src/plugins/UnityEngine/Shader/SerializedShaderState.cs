namespace UnityEngine
{
    public class SerializedShaderState
    {
        public string Name;
        public SerializedShaderRTBlendState[] RtBlend;
        public bool RtSeparateBlend;
        public SerializedShaderFloatValue ZClip;
        public SerializedShaderFloatValue ZTest;
        public SerializedShaderFloatValue ZWrite;
        public SerializedShaderFloatValue Culling;
        public SerializedShaderFloatValue Conservative;
        public SerializedShaderFloatValue OffsetFactor;
        public SerializedShaderFloatValue OffsetUnits;
        public SerializedShaderFloatValue AlphaToMask;
        public SerializedStencilOp StencilOp;
        public SerializedStencilOp StencilOpFront;
        public SerializedStencilOp StencilOpBack;
        public SerializedShaderFloatValue StencilReadMask;
        public SerializedShaderFloatValue StencilWriteMask;
        public SerializedShaderFloatValue StencilRef;
        public SerializedShaderFloatValue FogStart;
        public SerializedShaderFloatValue FogEnd;
        public SerializedShaderFloatValue FogDensity;
        public SerializedShaderVectorValue FogColor;
        public FogMode FogMode;
        public int GpuProgramID;
        public SerializedTagMap Tags;
        public int LOD;
        public bool Lighting;

    }

}
