using System.Security.Cryptography;
using System.Text;

namespace Cantarus.Libraries.Encryption
{
    /// <summary>
    /// Provides useful utility methods which may not easily be grouped
    /// elsewhere.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Generates a byte array of the length specified filled with random
        /// bytes.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GenerateRandomBytes(int length)
        {
            // Create a new byte array of the size required.
            byte[] bytes = new byte[length];

            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill it with random bytes.
                rngCsp.GetBytes(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Hashes the passed value using the SHA256 algorithm.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] SHA256Hash(string value)
        {
            // Convert string to byte array.
            byte[] bytes = Encoding.UTF8.GetBytes(value);

            byte[] hashedBytes;

            using (SHA256 sha = new SHA256Managed())
            {
                // Hash bytes.
                hashedBytes = sha.ComputeHash(bytes);
            }

            return hashedBytes;
        }
    }
}
