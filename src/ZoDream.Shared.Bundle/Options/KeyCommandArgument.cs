namespace ZoDream.Shared.Bundle
{
    public class KeyCommandArgument(byte[] key) : ICommandArgument
    {
        public const string TagName = "key";
        public byte[] Key => key;
    }
}
