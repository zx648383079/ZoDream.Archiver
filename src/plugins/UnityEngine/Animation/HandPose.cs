using System.Numerics;
using ZoDream.Shared.Numerics;

namespace UnityEngine
{
    public class HandPose
    {
        public Transform<Vector3> GrabX;
        public float[] DoFArray;
        public float Override;
        public float CloseOpen;
        public float InOut;
        public float Grab;
    }

}
