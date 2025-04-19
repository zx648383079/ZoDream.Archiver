namespace UnityEngine
{
    public class Clip
    {
        public ACLClip ACLClip { get; set; } = new EmptyACLClip();
        public StreamedClip StreamedClip { get; set; }
        public DenseClip DenseClip { get; set; }
        public ConstantClip ConstantClip { get; set; }
        public ValueArrayConstant Binding { get; set; }

    }
}
