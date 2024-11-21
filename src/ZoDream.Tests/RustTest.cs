using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.RustWrapper;

namespace ZoDream.Tests
{
    [TestClass]
    public class RustTest
    {
        [TestMethod]
        public void TestEncrypt()
        {
            using var encryptor = new Encryptor(EncryptionID.Unknown, "kkkk");
            var buffer = new byte[10];
            buffer[1] = 200;
            var res = encryptor.Encrypt(buffer);
            Assert.AreEqual(res.Length, buffer.Length);
            Assert.AreEqual(res[0], buffer[0] + 9);
            Assert.AreEqual(res[1], buffer[1] - 9);
        }
    }

}
