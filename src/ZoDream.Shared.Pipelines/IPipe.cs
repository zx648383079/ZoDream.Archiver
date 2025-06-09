namespace ZoDream.Shared.Pipelines
{
    public interface IPipe
    {

    }

    public interface IPipe<T> : IPipe
    {
        public T Handle(T stream);
    }

}