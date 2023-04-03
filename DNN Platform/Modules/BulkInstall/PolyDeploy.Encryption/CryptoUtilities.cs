namespace PolyDeploy.Encryption
{
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Provides useful utility methods which may not easily be grouped
    /// elsewhere.
    /// </summary>
    public static class CryptoUtilities
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
            var bytes = new byte[length];

            using RNGCryptoServiceProvider rngCsp = new();
            // Fill it with random bytes.
            rngCsp.GetBytes(bytes);

            return bytes;
        }

        public static string SHA256HashString(string value)
        {
            var bytes = SHA256HashBytes(value);

            return bytes.Aggregate("", (current, part) => $"{current}{part:X2}");
        }

        /// <summary>
        /// Hashes the passed value using the SHA256 algorithm.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] SHA256HashBytes(string value)
        {
            // Convert string to byte array.
            var bytes = Encoding.UTF8.GetBytes(value);

            using SHA256 sha = new SHA256Managed();
            // Hash bytes.
            var hashedBytes = sha.ComputeHash(bytes);

            return hashedBytes;
        }
    }
}
