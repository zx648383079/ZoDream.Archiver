namespace ZoDream.Shared.Bundle
{
    public class SkipCommandArgument(long length) : ICommandArgument
    {
        public const string TagName = "fake";

        public long Position => length;

        public override string ToString() => $"{TagName}:{length}";
    }
}
