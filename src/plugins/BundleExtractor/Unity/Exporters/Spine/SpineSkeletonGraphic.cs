using System.Numerics;
using UnityEngine;
using ZoDream.Shared.Numerics;
using Object = UnityEngine.Object;

namespace ZoDream.BundleExtractor.Unity.Spine
{
    internal class SpineSkeletonGraphic : MonoBehaviour
    {
        public IPPtr<Material> Material { get; set; }

        public ColorF Color { get; set; }

        public int RaycastTarget { get; set; }


        public Vector4 RaycastPadding { get; set; }


        public bool Maskable { get; set; }


        public SpineCullStateChangedEvent OnCullStateChanged { get; set; }


        public IPPtr<MonoBehaviour> SkeletonDataAsset { get; set; }


        public IPPtr<Material> AdditiveMaterial { get; set; }


        public IPPtr<Material> MultiplyMaterial { get; set; }


        public IPPtr<Material> ScreenMaterial { get; set; }


        public string InitialSkinName { get; set; }


        public bool InitialFlipX { get; set; }


        public bool InitialFlipY { get; set; }


        public string StartingAnimation { get; set; }


        public bool StartingLoop { get; set; }


        public float TimeScale { get; set; }


        public bool Freeze { get; set; }


        public int UpdateWhenInvisible { get; set; }


        public bool UnscaledTime { get; set; }


        public bool AllowMultipleCanvasRenderers { get; set; }


        public IPPtr<CanvasRenderer>[] CanvasRenderers { get; set; }


        public string[] SeparatorSlotNames { get; set; }


        public bool EnableSeparatorSlots { get; set; }


        public IPPtr<Transform>[] SeparatorParts { get; set; }


        public bool UpdateSeparatorPartLocation { get; set; }


        public SpineMeshGenerator MeshGenerator { get; set; }

        public struct SpineCullStateChangedEvent
        {

            public SpinePersistentCallGroup PersistentCalls { get; set; }
        }

        public struct SpinePersistentCallGroup
        {

            public SpinePersistentCall[] Calls { get; set; }
        }

        public struct SpinePersistentCall
        {
            public IPPtr<Object> Target { get; set; }
            public string TargetAssemblyTypeName { get; set; }
            public string MethodName { get; set; }
            public int Mode { get; set; }

            public SpineArgumentCache Arguments  { get; set; }
        }

        public class SpineArgumentCache
        {
            public IPPtr<Object> ObjectArgument { get; set; }
            public string ObjectArgumentAssemblyTypeName { get; set; }

            public int IntArgument { get; set; }
            public float FloatArgument { get; set; }
            public string StringArgument { get; set; }
            public bool BoolArgument { get; set; }
        }

        public struct SpineMeshGenerator
        {

            public SpineSettings Settings { get; set; }

        }

        public struct SpineSettings
        {

            public bool UseClipping { get; set; }


            public float ZSpacing { get; set; }


            public bool PmaVertexColors { get; set; }


            public bool TintBlack { get; set; }


            public bool CanvasGroupTintBlack { get; set; }


            public bool CalculateTangents { get; set; }


            public bool AddNormals { get; set; }


            public bool ImmutableTriangles { get; set; }
        }
    }

  
}
