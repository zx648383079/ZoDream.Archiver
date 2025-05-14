using UnityEngine;

namespace ZoDream.BundleExtractor.Unity.Spine
{
    internal class SpineSkeletonDataAsset : MonoBehaviour
    {
        public IPPtr<MonoBehaviour>[] AtlasAssets { get; set; }

        public float Scale { get; set; }
        public IPPtr<TextAsset> SkeletonJSON { get; set; }
        public bool IsUpgradingBlendModeMaterials { get; set; }

        public SpineBlendModeMaterials BlendModeMaterials { get; set; }

        //public object[] SkeletonDataModifiers { get; set; }

        public string[] FromAnimation { get; set; }

        public string[] ToAnimation { get; set; }

        public float[] Duration { get; set; }

        public float DefaultMix { get; set; }

        public IPPtr<RuntimeAnimatorController> Controller { get; set; }

        public struct SpineBlendModeMaterials
        {

            public bool RequiresBlendModeMaterials { get; set; }


            public bool ApplyAdditiveMaterial { get; set; }


            public SpineReplacementMaterial[] AdditiveMaterials { get; set; }


            public SpineReplacementMaterial[] MultiplyMaterials { get; set; }


            public SpineReplacementMaterial[] ScreenMaterials { get; set; }
        }
        public struct SpineReplacementMaterial
        {

            public string PageName { get; set; }


            public IPPtr<Material> Material { get; set; }
        }
    }
}
