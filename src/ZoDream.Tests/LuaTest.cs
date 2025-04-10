using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.LuaDecompiler;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;

namespace ZoDream.Tests
{
    [TestClass]
    public class LuaTest
    {

        [TestMethod]
        public void TestDecompiler()
        {
            var root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../zodream/tests"));
            var path = Path.Combine(root, "lua53//test.luac");
            var scheme = new LuaScheme();
            using var fs = File.OpenRead(path);
            var res = scheme.Open(fs);
            Assert.IsNotNull(res);
            using var sb = new CodeWriter();
            new LuaWriter(res).Decompile(sb);
            var str = sb.ToString();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(str));
        }

        [TestMethod]
        public void TestOp()
        {
            Assert.IsTrue(JitOperandExtractor.IsABCFormat(JitOperand.MODNV));
        }
    }
}
