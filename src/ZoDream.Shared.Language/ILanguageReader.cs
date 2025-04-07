namespace ZoDream.Shared.Language
{
    public interface ILanguageReader<T> 
        where T : IBytecode
    {
        public T Read();
    }
}
