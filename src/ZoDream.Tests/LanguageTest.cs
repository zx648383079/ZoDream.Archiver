using ZoDream.Shared.Language;
using ZoDream.SourceGenerator;

namespace ZoDream.Tests
{
    [TestClass]
    public class LanguageTest
    {

        [TestMethod]
        public void TestPattern()
        {
            var root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../imhex/patterns"));
            var fileName = Path.Combine(root, "unity//mesh.hexpat");
            using var fs = File.OpenRead(fileName);
            var lexer = new PatternLanguageLexer(new StreamReader(fs));
            var items = new List<Token>();
            while (true)
            {
                var token = lexer.NextToken();
                items.Add(token);
                if (token.Type == TokenType.Eof)
                {
                    break;
                }
            }
            Assert.IsTrue(items.Count == 10);
        }
    }
}
