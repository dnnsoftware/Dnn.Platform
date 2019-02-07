using Cantarus.Libraries.Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace EncryptionTests
{
    [TestClass]
    public class EncryptionTests
    {
        private const int Iterations = 100;

        [TestMethod]
        public void EncryptString_RandomString_ObfuscatedAfterEncryption()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = TestUtilities.GeneratePassPhrase();
                string beforeString = Encoding.UTF8.GetString(TestUtilities.GeneratePayload());

                string encryptedString = Crypto.Encrypt(beforeString, passPhrase);

                Assert.AreNotEqual(beforeString, encryptedString);
            }
        }

        [TestMethod]
        public void EncryptBytes_RandomBytes_ObfuscatedAfterEncryption()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = TestUtilities.GeneratePassPhrase();
                byte[] beforeBytes = TestUtilities.GeneratePayload();

                byte[] encryptedBytes = Crypto.Encrypt(beforeBytes, passPhrase);

                CollectionAssert.AreNotEqual(beforeBytes, encryptedBytes);
            }
        }

        [TestMethod]
        public void EncryptStream_StreamOfRandomBytes_ObfuscatedAfterEncryption()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = TestUtilities.GeneratePassPhrase();
                byte[] beforeBytes = TestUtilities.GeneratePayload();
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
