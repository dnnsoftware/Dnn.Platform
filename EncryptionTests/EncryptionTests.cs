using Cantarus.Libraries.Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace EncryptionTests
{
    [TestClass]
    public class EncryptionTests : TestBase
    {
        [TestMethod]
        public void String()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = GeneratePassPhrase();
                string beforeString = Encoding.UTF8.GetString(GeneratePayload());

                string encryptedString = Crypto.Encrypt(beforeString, passPhrase);

                Assert.AreNotEqual(beforeString, encryptedString);
            }
        }

        [TestMethod]
        public void Byte()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = GeneratePassPhrase();
                byte[] beforeBytes = GeneratePayload();

                byte[] encryptedBytes = Crypto.Encrypt(beforeBytes, passPhrase);

                CollectionAssert.AreNotEqual(beforeBytes, encryptedBytes);
            }
        }

        [TestMethod]
        public void Stream()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = GeneratePassPhrase();
                byte[] beforeBytes = GeneratePayload();
                byte[] encryptedBytes;

                using (MemoryStream plainStream = new MemoryStream(beforeBytes))
                {
                    using (MemoryStream encryptedSteam = (MemoryStream)Crypto.Encrypt(plainStream, passPhrase))
                    {
                        encryptedBytes = encryptedSteam.ToArray();
                    }
                }

                CollectionAssert.AreNotEqual(beforeBytes, encryptedBytes);
            }
        }
    }
}
