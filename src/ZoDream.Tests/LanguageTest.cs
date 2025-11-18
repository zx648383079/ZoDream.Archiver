using System.Text.Json;
using System.Text.RegularExpressions;
using UnityEngine.Document;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
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
            var fileName = Path.Combine(root, "unity/mesh.hexpat");
            using var fs = File.OpenRead(fileName);
            var lexer = new PatternLanguageLexer(new StreamReader(fs));
            var writer = new SourceWriter(lexer);
            using var sb = new CodeWriter();
            writer.Write(sb);
            var res = sb.ToString();
            Assert.IsTrue(res.Length > 0);
        }

        [TestMethod]
        public void TestParse()
        {
            var fileName = "2021.3.40f1.json";
            if (!File.Exists(fileName))
            {
                return;
            }
            var data = TypeNodeReader.LoadFile(fileName);
            var writer = new TypeSourceWriter(data);
            using var sb = new CodeWriter();
            writer.Write(sb);
            var res = sb.ToString();
            Assert.IsTrue(res.Length > 0);
        }

        //[TestMethod]
        public void TestExtract()
        {
            var root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../src/plugins"));
            var folder = Path.Combine(root, "BundleExtractor/Unity/Converters");
            var entry = new BundleSource([folder]);
            using var sb = new CodeWriter();
            var regex = new Regex(@"class\s+(.+?Converter)\s+:");
            foreach (var item in entry.GetFiles())
            {
                var text = File.ReadAllText(item);
                foreach (Match match in regex.Matches(text))
                {
                    sb.WriteFormat("new {0}(),", match.Groups[1].Value).WriteLine(true);
                }
            }
            var res = sb.ToString();
            Assert.IsTrue(res.Length > 0);
        }
        [TestMethod]
        public void TestCompare()
        {
            var rootFolder = "D:\\Downloads\\d";
            var data = TypeNodeReader.LoadFile(Path.Combine(rootFolder, "2022.3.27f1.json"));
            var next = TypeNodeReader.LoadFile(Path.Combine(rootFolder, "2022.3.26f1.json"));
            var comparer = new TypeNodeComparer();
            var res = comparer.Compare(data, next);
            Assert.IsTrue(res?.Children?.Length > 0);
        }

    }
}
