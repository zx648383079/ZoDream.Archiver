namespace UnityEngine
{
    public sealed class Animator : Behaviour
    {
        public IPPtr<Avatar> Avatar;
        public IPPtr<RuntimeAnimatorController> Controller;
        public bool HasTransformHierarchy = true;
    }
}
