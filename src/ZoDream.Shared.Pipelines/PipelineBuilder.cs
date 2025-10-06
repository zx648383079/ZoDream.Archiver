using System;

namespace ZoDream.Shared.Pipelines
{
    public class PipelineBuilder : IPipelineBuilder
    {


        public IPipelineBuilder Add(IPipelineMatcher matcher, IPipelineBehavior behavior)
        {

            return this;
        }

        public IPipelineBehavior BuildBehavior()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public IPipelineMatcher MatchFile(string pattern)
        {
            return new FileMatcher(pattern);
        }

        public IPipelineMatcher MatchFolder(string pattern)
        {
            return new FolderMatcher(pattern);
        }
    }
}
