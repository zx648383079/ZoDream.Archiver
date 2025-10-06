using System.IO.Enumeration;

namespace ZoDream.Shared.Pipelines
{
    public class FileMatcher(string pattern) : IPipelineMatcher
    {
        public bool IsMatch(string fileName)
        {
            return FileSystemName.MatchesSimpleExpression(pattern, fileName, true);
        }
    }

    public class FolderMatcher(string pattern) : IPipelineMatcher
    {
        public bool IsMatch(string filePath)
        {
            return FileSystemName.MatchesSimpleExpression(pattern, filePath, true);
        }
    }
}
