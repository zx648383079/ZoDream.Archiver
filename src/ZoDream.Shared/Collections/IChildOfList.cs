namespace ZoDream.Shared.Collections
{
    public interface IChildOfList<TParent>
        where TParent : class
    {
        public TParent? LogicalParent { get; }

        public int LogicalIndex { get; }
        public void SetLogicalParent(TParent? parent, int index);
    }
}
