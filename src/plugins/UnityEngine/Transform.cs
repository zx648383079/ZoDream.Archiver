﻿using System.Numerics;

namespace UnityEngine
{
    public class Transform : Component
    {
        public Quaternion LocalRotation;
        public Vector3 LocalPosition;
        public Vector3 LocalScale;
        public IPPtr<Transform>[] Children;
        public IPPtr<Transform> Father;
    }


}