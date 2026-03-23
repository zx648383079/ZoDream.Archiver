namespace ZoDream.Live2dExporter.Models
{
    public class JsonMotionSyncSetting
    {
        public string Id { get; set; }

        public string AnalysisType { get; set; }

        public string UseCase { get; set; }

        public JsonMotionCubismParameter[] CubismParameters { get; set; }

        public JsonMotionAudioParameter[] AudioParameters { get; set; }

        public JsonMotionMappingParameter[] Mappings { get; set; }

        public JsonMotionPostProcessing PostProcessing { get; set; }
    }
}