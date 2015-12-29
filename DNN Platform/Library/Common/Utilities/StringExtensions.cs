using System.Text;

namespace DotNetNuke.Common.Utilities
{
    public static class StringExtensions
    {
        public static string TrimToLength(this string source, int maxLength)
        {
            return source == null || source.Length <= maxLength
                ? source :
                source.Substring(0, maxLength);
        }

        public static string Append(this string stringValue, string stringToLink, string delimiter)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                return stringToLink;
            }
            if (string.IsNullOrEmpty(stringToLink))
            {
                return stringValue;
            }
            return stringValue + delimiter + stringToLink;
        }

        public static string ValueOrEmpty(this string stringValue)
        {
            return stringValue ?? string.Empty;
        }

        /// <summary>
        /// Takes a string and removes any diacritics
        /// </summary>
        /// <param name="input">String to normalize</param>
        /// <returns>String without diacritics</returns>
		public static string NormalizeString(this string input)
		{
			return string.IsNullOrEmpty(input)
                ? input
                : Iso8859Encoding.GetString(Encoding.Convert(Encoding.UTF8, Iso8859Encoding, Encoding.UTF8.GetBytes(input))).ToLower();
		}

        private static readonly Encoding Iso8859Encoding = Encoding.GetEncoding("iso-8859-8");
    }

}
