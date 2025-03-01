namespace ZoDream.KhronosExporter.Models
{
    internal class ObjFaceVertex
    {
        /// <summary>
        /// vertex coordinates index
        /// </summary>
        public int V;
        /// <summary>
        /// vertex texture coordinates index
        /// </summary>
        public int T;
        /// <summary>
        /// vertex normal index
        /// </summary>
        public int N;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v">vertex coordinates index</param>
        public ObjFaceVertex(int v) : this(v, 0, 0) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v">vertex coordinates index</param>
        /// <param name="n">vertex normal index</param>
        public ObjFaceVertex(int v, int n) : this(v, 0, n) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v">vertex coordinates index</param>
        /// <param name="t">vertex texture coordinates index</param>
        /// <param name="n">vertex normal index</param>
        public ObjFaceVertex(int v, int t, int n)
        {
            V = v;
            N = n;
            T = t;
        }

        public override string ToString()
        {
            return N > 0 ? T > 0 ? $"{V}/{T}/{N}" : $"{V}//{N}" : T > 0 ? $"{V}/{T}" : $"{V}";
        }

        public bool Equals(ObjFaceVertex other)
        {
            return V == other.V && T == other.T && N == other.N;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not ObjFaceVertex o)
            {
                return false;
            }
            return Equals(o);
        }

        public override int GetHashCode()
        {
            return V ^ T ^ N;
        }
    }
}