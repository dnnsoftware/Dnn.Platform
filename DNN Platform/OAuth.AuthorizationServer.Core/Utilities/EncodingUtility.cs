using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace OAuth.AuthorizationServer.Core.Utilities
{
    /// <summary>
    /// oauth encoding utility class
    /// </summary>
    public class EncodingUtility
    {
        /// <summary>
        /// url encode and ensure tamperproof string 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string Encode(string value, string Key)
        {
            return  HttpUtility.UrlEncode(TamperProofStringEncode(value, Key));
        }

        /// <summary>
        /// urldecode and tamperproof string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string Decode(string value, string Key)
        {
            return TamperProofStringDecode(HttpUtility.UrlDecode(value), Key);
        }

        // Function to encode the string
        private static string TamperProofStringEncode(string value, string Key)
        {
            string strKey = null;

            MACTripleDES mac3des = new MACTripleDES();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            mac3des.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(Key));
            strKey = Convert.ToBase64String(mac3des.ComputeHash(Encoding.UTF8.GetBytes(value)));

            // convert key to hex because we can't have goofy characters in the query string
            strKey = EncodeHexString(strKey);

            // Cleanup
            mac3des.Clear();
            md5.Clear();

            return Convert.ToString(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) + '-') + strKey;
        }

        // encodes a string to its equivalent hex value string
        private static string EncodeHexString(string sText)
        {
            int intLength = sText.Length;
            if (intLength == 0)
            {
                return string.Empty;
            }
            int intCount = 0;
            StringBuilder sb = new StringBuilder(intLength * 2);
            byte[] bBytes = Encoding.ASCII.GetBytes(sText);
            for (intCount = 0; intCount <= bBytes.Length - 1; intCount++)
            {
                sb.AppendFormat("{0:X2}", bBytes[intCount]);
            }
            return sb.ToString();
        }

        // decodes a string that is hex back to being a standard string
        private static string DecodeHexString(string sText)
        {
            int intLength = sText.Length;
            if (intLength == 0)
            {
                return string.Empty;
            }
            int intCount = 0;
            StringBuilder sb = new StringBuilder(Convert.ToInt32(intLength / 2));
            try
            {
                intCount = 0;
                while (intCount < sText.Length - 1)
                {
                    sb.Append(Convert.ToChar(Byte.Parse(sText.Substring(intCount, 2), System.Globalization.NumberStyles.HexNumber)));
                    intCount = intCount + 2;
                }
            }
            catch
            {
                return string.Empty;
            }

            return sb.ToString();
        }


        // Function to decode the string
        // Throws an exception if the data is corrupt
        private static string TamperProofStringDecode(string value, string key)
        {
            string dataValue = string.Empty;
            string calcHash = string.Empty;
            string storedHash = string.Empty;
            string strKey = null;
            MACTripleDES mac3des = new MACTripleDES();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            mac3des.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(key));

            try
            {
                dataValue = Encoding.UTF8.GetString(Convert.FromBase64String(value.Split('-')[0]));
                strKey = value.Split('-')[1];
                strKey = DecodeHexString(strKey);
                storedHash = Encoding.UTF8.GetString(Convert.FromBase64String(strKey));
                calcHash = Encoding.UTF8.GetString(mac3des.ComputeHash(Encoding.UTF8.GetBytes(dataValue)));

                if (storedHash != calcHash)
                {
                    // Data was corrupted

                    // This error is immediately caught below
                    throw new ArgumentException("Hash value does not match");
                }
            }
            catch
            {
                throw new ArgumentException(Convert.ToString((Convert.ToString("Invalid TamperProofString stored hash = ") + storedHash) + " calchash = ") + calcHash);
            }

            // Cleanup
            mac3des.Clear();
            md5.Clear();

            return dataValue;
        }
    }
}
