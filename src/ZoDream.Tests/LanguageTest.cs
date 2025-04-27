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
            var writer = new SourceWriter(lexer);
            using var sb = new CodeWriter();
            writer.Write(sb);
            var res = sb.ToString();
            Assert.IsTrue(res.Length > 0);
        }
    }
}
