using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using ClientDependency.Core.Config;

namespace ClientDependency.Core
{
    public static class StringExtensions
    {
        /// <summary>
        /// Generally used for unit tests to get access to the settings
        /// </summary>
        internal static Func<ClientDependencySection> GetConfigSection;

        internal static string ReplaceNonAlphanumericChars(this string input, char replacement)
        {
            //any character that is not alphanumeric, convert to a hyphen
            var mName = input;
            foreach (var c in mName.ToCharArray().Where(c => !char.IsLetterOrDigit(c)))
            {
                mName = mName.Replace(c, replacement);
            }
            return mName;
        }

        public static string ReverseString(this string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string EncodeTo64Url(this string toEncode)
        {
            string returnValue = EncodeTo64(toEncode);

            // returnValue is base64 = may contain a-z, A-Z, 0-9, +, /, and =.
            // the = at the end is just a filler, can remove
            // then convert the + and / to "base64url" equivalent
            //
            returnValue = returnValue.TrimEnd(new char[] { '=' });
            returnValue = returnValue.Replace("+", "-");
            returnValue = returnValue.Replace("/", "_");

            return returnValue;
        }

        public static string EncodeTo64(this string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        public static string DecodeFrom64Url(this string toDecode)
        {
            // see BaseFileRegistrationProvider.EncodeTo64Url
            //
            toDecode = toDecode.Replace("-", "+");
            toDecode = toDecode.Replace("_", "/");
            int rem = toDecode.Length % 4; // 0 (aligned), 1, 2 or 3 (not aligned)
            if (rem > 0)
                toDecode = toDecode.PadRight(toDecode.Length + 4 - rem, '='); // align

            return DecodeFrom64(toDecode);
        }

        public static string DecodeFrom64(this string toDecode)
        {
            byte[] toDecodeAsBytes = System.Convert.FromBase64String(toDecode);
            return Encoding.UTF8.GetString(toDecodeAsBytes);
        }

        /// <summary>
        /// Generates a hash of a string based on the FIPS compliance setting.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GenerateHash(this string str)
        {
            return CryptoConfig.AllowOnlyFipsAlgorithms
                ? str.GenerateSha1Hash()
                : str.GenerateMd5();
        }

        /// <summary>
        /// Generate a SHA1 hash of a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GenerateSha1Hash(this string str)
        {
            return str.GenerateHash("SHA1");
        }

        [Obsolete("This is no longer used and will be removed in future versions")]
        public static string GenerateSha256Hash(this string str)
        {
            return str.GenerateHash("SHA256");
        }

        /// <summary>Generate a MD5 hash of a string
        /// </summary>
        public static string GenerateMd5(this string str)
        {
            return str.GenerateHash("MD5");
        }

        /// <summary>Generate a MD5 hash of a string
        /// </summary>
        private static string GenerateHash(this string str, string hashType)
        {
            var hasher = HashAlgorithm.Create(hashType);
            if (hasher == null) throw new InvalidOperationException("No hashing type found by name " + hashType);
            using (hasher)
            {
                //convert our string into byte array
                var byteArray = Encoding.UTF8.GetBytes(str);

                //get the hashed values created by our SHA1CryptoServiceProvider
                var hashedByteArray = hasher.ComputeHash(byteArray);

                //create a StringBuilder object
                var stringBuilder = new StringBuilder();

                //loop to each each byte
                foreach (var b in hashedByteArray)
                {
                    //append it to our StringBuilder
                    stringBuilder.Append(b.ToString("x2").ToLower());
                }

                //return the hashed value
                return stringBuilder.ToString();
            }
        }


        /// <summary>
        /// checks if the string ends with one of the strings specified. This ignores case.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static bool EndsWithOneOf(this string str, string[] ext)
        {
            var upper = str.ToUpper();
            bool isExt = false;
            foreach (var e in ext)
            {
                if (upper.EndsWith(e.ToUpper()))
                {
                    isExt = true;
                    break;
                }
            }
            return isExt;
        }

    }
}
