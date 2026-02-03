namespace ZoDream.Shared.Bundle
{
    public class EmptyCommandArgument : ICommandArgument
    {
        public static EmptyCommandArgument Instance = new();
        public override string ToString()
        {
            return "<Empty>";
        }
    }

    public class UnknownCommandArgument : ICommandArgument
    {
        public const string TagName = "unknown";
        public static UnknownCommandArgument Instance = new();

        public override string ToString()
        {
            return TagName;
        }
    }
}
