using System.Numerics;

namespace ZoDream.Shared.Numerics
{
    public struct Transform<T> where T : struct
    {
        public T Translation;
        public Quaternion Rotation;
        public T Scale;

        public Transform()
        {
            
        }

        public Transform(T t, Quaternion q, T s)
        {
            Translation = t;
            Rotation = q;
            Scale = s;
        }
    }
}
