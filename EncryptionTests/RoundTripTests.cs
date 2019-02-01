using Cantarus.Libraries.Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace EncryptionTests
{
    [TestClass]
    public class RoundTripTests
    {
        private const int Iterations = 100;

        [TestMethod]
        public void RoundTrip_RandomString_MatchesAfterEncryptDecrypt()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = TestUtilities.GeneratePassPhrase();
                string beforeString = Encoding.UTF8.GetString(TestUtilities.GeneratePayload());

                string encryptedString = Crypto.Encrypt(beforeString, passPhrase);

                string afterString = Crypto.Decrypt(encryptedString, passPhrase);

                Assert.AreEqual(beforeString, afterString);
            }
        }

        [TestMethod]
        public void RoundTrip_RandomBytes_MatchesAfterEncryptDecrypt()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = TestUtilities.GeneratePassPhrase();
                byte[] beforeBytes = TestUtilities.GeneratePayload();

                byte[] encryptedBytes = Crypto.Encrypt(beforeBytes, passPhrase);

                byte[] afterBytes = Crypto.Decrypt(encryptedBytes, passPhrase);

                CollectionAssert.AreEqual(beforeBytes, afterBytes);
            }
        }

        [TestMethod]
        public void RoundTrip_RandomStreamOfBytes_MatchesAfterEncryptDecrypt()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = TestUtilities.GeneratePassPhrase();
                byte[] beforeBytes = TestUtilities.GeneratePayload();
                byte[] afterBytes;

                using (MemoryStream plainStream = new MemoryStream(beforeBytes))
                {
                    using (Stream encryptedSteam = Crypto.Encrypt(plainStream, passPhrase))
                    {
                        using (MemoryStream decryptedStream = (MemoryStream)Crypto.Decrypt(encryptedSteam, passPhrase))
                        {
                            afterBytes = decryptedStream.ToArray();
                        }
                    }
                }

                CollectionAssert.AreEqual(beforeBytes, afterBytes);
            }
        }
    }
}
