namespace ZoDream.Shared.Pipelines
{
    public class MultipleMatcher(IPipelineMatcher[] items) : IMultipleMatcher
    {
        public bool IsMatch()
        {
            return false;
        }
    }
}
