using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.LuaDecompiler;
using ZoDream.LuaDecompiler.Models;

namespace ZoDream.Tests
{
    [TestClass]
    public class LuaTest
    {

        [TestMethod]
        public void TestDecompiler()
        {
            var root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../zodream/tests"));
            var path = Path.Combine(root, "luajit//float.luac");
            var scheme = new LuaScheme();
            using var fs = File.OpenRead(path);
            var res = scheme.Open(fs, string.Empty, string.Empty);
            Assert.IsNotNull(res);
        }

        [TestMethod]
        public void TestOp()
        {
            Assert.IsTrue(JitOperandExtractor.IsABCFormat(JitOperand.MODNV));
        }
    }
}
