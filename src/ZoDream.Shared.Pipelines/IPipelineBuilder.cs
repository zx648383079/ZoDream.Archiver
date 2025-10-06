using System;

namespace ZoDream.Shared.Pipelines
{
    public interface IPipelineBuilder : IDisposable
    {

        public IPipelineBuilder Add(IPipelineMatcher matcher, IPipelineBehavior behavior);

        public IPipelineBehavior BuildBehavior();
    }
}
