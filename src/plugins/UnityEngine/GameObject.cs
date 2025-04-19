
namespace UnityEngine
{
    public class GameObject : Object
    {
        public PPtr<Component>[] Components;

        public Transform Transform;
        public MeshRenderer MeshRenderer;
        public MeshFilter MeshFilter;
        public SkinnedMeshRenderer SkinnedMeshRenderer;
        public Animator Animator;
        public Animation Animation;
    }
}
