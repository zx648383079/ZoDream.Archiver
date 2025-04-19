namespace UnityEngine
{
    public sealed class Animator : Behaviour
    {
        public PPtr<Avatar> Avatar;
        public PPtr<RuntimeAnimatorController> Controller;
        public bool HasTransformHierarchy = true;
    }
}
