using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace EncryptionTests
{
    [TestClass]
    internal static class TestUtilities
    {
        private static Random rand;

        private static Random Rand
        {
            get
            {
                if (rand == null)
                {
                    rand = new Random();
                }

                return rand;
            }
        }

        public static string GeneratePassPhrase()
        {
            return GenerateRandomString(Rand.Next(8, (256 + 1)));
        }

        public static byte[] GeneratePayload()
        {
            return GenerateRandomBytes(Rand.Next(16, (512 + 1)));
        }

        private static string GenerateRandomString(int length)
        {
            byte[] bytes = new byte[length];

            for (int i = 0; i < bytes.Length; i++)
            {
                int min;
                int max;

                if (Rand.Next(0, 2) > 0)
                {
                    min = 65;
                    max = 90;
                }
                else
                {
                    min = 97;
                    max = 122;
                }

                int num = Rand.Next(min, max);

                bytes[i] = (byte)num;
            }

            return Encoding.UTF8.GetString(bytes);
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];

            Rand.NextBytes(bytes);

            return bytes;
        }
    }
}
