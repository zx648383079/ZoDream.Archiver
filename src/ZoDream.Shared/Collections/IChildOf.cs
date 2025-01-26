namespace ZoDream.Shared.Collections
{
    public interface IChildOf<TParent>
        where TParent : class
    {
        public TParent LogicalParent { get; }

        public void SetLogicalParent(TParent parent);
    }
}
