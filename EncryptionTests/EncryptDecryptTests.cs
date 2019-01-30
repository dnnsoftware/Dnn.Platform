using Cantarus.Libraries.Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace EncryptionTests
{
    [TestClass]
    public class EncryptDecryptTests
    {
        private const int Iterations = 1000;

        [TestMethod]
        public void StringEncryptDecrypt()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = GeneratePassPhrase();
                string beforeString = Encoding.UTF8.GetString(GeneratePayload());

                string encryptedString = Crypto.Encrypt(beforeString, passPhrase);

                string afterString = Crypto.Decrypt(encryptedString, passPhrase);

                Assert.AreEqual(beforeString, afterString);
            }
        }

        [TestMethod]
        public void ByteEncryptDecrypt()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = GeneratePassPhrase();
                byte[] beforeBytes = GeneratePayload();

                byte[] encryptedBytes = Crypto.Encrypt(beforeBytes, passPhrase);

                byte[] afterBytes = Crypto.Decrypt(encryptedBytes, passPhrase);

                CollectionAssert.AreEqual(beforeBytes, afterBytes);
            }
        }

        [TestMethod]
        public void StreamEncryptDecrypt()
        {
            for (int i = 0; i < Iterations; i++)
            {
                string passPhrase = GeneratePassPhrase();
                byte[] beforeBytes = GeneratePayload();
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

        private string GeneratePassPhrase()
        {
            Random rand = new Random();

            return GenerateRandomString(rand.Next(8, (256 + 1)));
        }

        private byte[] GeneratePayload()
        {
            Random rand = new Random();

            return GenerateRandomBytes(rand.Next(16, (512 + 1)));
        }

        private string GenerateRandomString(int length)
        {
            byte[] bytes = new byte[length];

            Random rand = new Random();

            for (int i = 0; i < bytes.Length; i++)
            {
                int min;
                int max;

                if (rand.Next(0, 2) > 0)
                {
                    min = 65;
                    max = 90;
                }
                else
                {
                    min = 97;
                    max = 122;
                }

                int num = rand.Next(min, max);

                bytes[i] = (byte)num;
            }

            return Encoding.UTF8.GetString(bytes);
        }

        private byte[] GenerateRandomBytes(int length)
        {
            Random rand = new Random();

            byte[] bytes = new byte[length];

            rand.NextBytes(bytes);

            return bytes;
        }
    }
}
