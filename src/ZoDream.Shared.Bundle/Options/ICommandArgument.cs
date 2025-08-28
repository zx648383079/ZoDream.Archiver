using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Bundle
{
    public interface ICommandArgument
    {
    }

    public interface ICommandArguments
    {

        public bool Contains(string cmd);
        public void Add(string cmd, ICommandArgument args);
        public bool TryGet<T>(string cmd, [NotNullWhen(true)] out T? args) where T : ICommandArgument;
    }
}
