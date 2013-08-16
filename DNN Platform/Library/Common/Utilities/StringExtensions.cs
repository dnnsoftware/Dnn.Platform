using System;

namespace DotNetNuke.Common.Utilities
{
    public static class StringExtensions
    {

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

    }

}
