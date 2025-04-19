namespace UnityEngine
{
    public class SerializedShader
    {
        public SerializedProperties PropInfo;
        public SerializedSubShader[] SubShaders;
        public string[] KeywordNames;
        public byte[] KeywordFlags;
        public string Name;
        public string CustomEditorName;
        public string FallbackName;
        public SerializedShaderDependency[] Dependencies;
        public SerializedCustomEditorForRenderPipeline[] CustomEditorForRenderPipelines;
        public bool DisableNoSubshadersMessage;

    }

}
