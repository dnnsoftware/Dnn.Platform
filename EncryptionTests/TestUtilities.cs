using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace EncryptionTests
{
    [TestClass]
    internal static class TestUtilities
    {
        public static string GeneratePassPhrase()
        {
            Random rand = new Random();

            return GenerateRandomString(rand.Next(8, (256 + 1)));
        }

        public static byte[] GeneratePayload()
        {
            Random rand = new Random();

            return GenerateRandomBytes(rand.Next(16, (512 + 1)));
        }

        private static string GenerateRandomString(int length)
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

        private static byte[] GenerateRandomBytes(int length)
        {
            Random rand = new Random();

            byte[] bytes = new byte[length];

            rand.NextBytes(bytes);

            return bytes;
        }
    }
}
