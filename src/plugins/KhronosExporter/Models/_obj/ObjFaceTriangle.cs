namespace ZoDream.KhronosExporter.Models
{
    internal class ObjFaceTriangle
    {
        /// <summary>
        /// The first vertex
        /// </summary>
        public ObjFaceVertex V1;
        /// <summary>
        /// The second vertex
        /// </summary>
        public ObjFaceVertex V2;
        /// <summary>
        /// The third vertex
        /// </summary>
        public ObjFaceVertex V3;

        public ObjFaceTriangle(ObjFaceVertex v1, ObjFaceVertex v2, ObjFaceVertex v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }
    }
}