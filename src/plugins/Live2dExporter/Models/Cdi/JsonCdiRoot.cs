namespace ZoDream.Live2dExporter.Models
{
    internal class JsonCdiRoot
    {
        public int Version { get; set; }
        public JsonCdiParameter[] Parameters { get; set; }
        public JsonCdiParameter[] ParameterGroups { get; set; }
        public JsonCdiPart[] Parts { get; set; }
        public string[][] CombinedParameters { get; set; }
    }
    
}
