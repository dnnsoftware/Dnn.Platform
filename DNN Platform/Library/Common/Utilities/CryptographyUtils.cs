// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>CryptographyUtils is a Utility class that provides Cryptography Utility constants.</summary>
    public static class CryptographyUtils
    {
        private const string SHA1Cng = "System.Security.Cryptography.SHA1Cng";

        private const string SHA1CryptoServiceProvider = "System.Security.Cryptography.SHA1CryptoServiceProvider";

        private const string SHA256Cng = "System.Security.Cryptography.SHA256Cng";

        private const string SHA256CryptoServiceProvider = "System.Security.Cryptography.SHA256CryptoServiceProvider";

        private const string SHA384Cng = "System.Security.Cryptography.SHA384Cng";

        private const string SHA384CryptoServiceProvider = "System.Security.Cryptography.SHA384CryptoServiceProvider";

        private const string SHA512Cng = "System.Security.Cryptography.SHA512Cng";

        private const string SHA512CryptoServiceProvider = "System.Security.Cryptography.SHA512CryptoServiceProvider";

        public static SHA1 CreateSHA1()
        {
            return SHA1.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? SHA1CryptoServiceProvider : SHA1Cng);
        }

        public static SHA256 CreateSHA256()
        {
            return SHA256.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? SHA256CryptoServiceProvider : SHA256Cng);
        }

        public static SHA384 CreateSHA384()
        {
            return SHA384.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? SHA384CryptoServiceProvider : SHA384Cng);
        }

        public static SHA512 CreateSHA512()
        {
            return SHA512.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? SHA512CryptoServiceProvider : SHA512Cng);
        }

        public static string GenerateHash(this string str)
        {
            try
            {
                return CryptoConfig.AllowOnlyFipsAlgorithms
                    ? str.GenerateSha256Hash()
                    : str.GenerateMd5();
            }
            catch (Exception)
            {
                return str.GenerateMd5();
            }
        }

        public static string GenerateSha256Hash(this string str)
        {
            return str.GenerateHash(CreateSHA256());
        }

        public static string GenerateMd5(this string str)
        {
            return str.GenerateHash("MD5");
        }

        public static string GenerateHash(this string str, string hashType)
        {
            var hasher = HashAlgorithm.Create(hashType);
            if (hasher == null)
            {
                throw new InvalidOperationException("No hashing type found by name " + hashType);
            }

            return str.GenerateHash(hasher);
        }

        public static string GenerateHash(this string str, HashAlgorithm hasher)
        {
            using (hasher)
            {
                // convert our string into byte array
                var byteArray = Encoding.UTF8.GetBytes(str);

                // get the hashed values created by our SHA1CryptoServiceProvider
                var hashedByteArray = hasher.ComputeHash(byteArray);

                // create a StringBuilder object
                var stringBuilder = new StringBuilder();

                // loop to each each byte
                foreach (var b in hashedByteArray)
                {
                    // append it to our StringBuilder
                    stringBuilder.Append(b.ToString("x2").ToLowerInvariant());
                }

                // return the hashed value
                return stringBuilder.ToString();
            }
        }
    }
}
