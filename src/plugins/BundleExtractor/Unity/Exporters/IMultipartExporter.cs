using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal interface IMultipartExporter : IBundleExporter, IDisposable
    {

        public bool IsEmpty { get; }

        public void Append(GameObject obj);
        public void Append(Mesh mesh);
        public void Append(Animator animator);
        public void Append(AnimationClip animator);

    }
}
