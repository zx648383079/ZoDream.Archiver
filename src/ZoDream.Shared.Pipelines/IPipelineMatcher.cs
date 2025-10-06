using System;
using System.IO;

namespace ZoDream.Shared.Pipelines
{
    public interface IPipelineMatcher
    {
    }

    public interface IStreamMatcher : IPipelineMatcher
    {

        public bool IsMatch(Stream input);
    }

    public interface IByteMatcher : IPipelineMatcher
    {

        public bool IsMatch(ReadOnlySpan<byte> input);
    }

    public interface IMultipleMatcher : IPipelineMatcher
    {

    }
}