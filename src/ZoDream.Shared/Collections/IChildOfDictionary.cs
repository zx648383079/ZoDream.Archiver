namespace ZoDream.Shared.Collections
{
    public interface IChildOfDictionary<TParent>
        where TParent : class
    {
        public TParent? LogicalParent { get; }
        public string? LogicalKey { get; }
        public void SetLogicalParent(TParent? parent, string? key);
    }
}
