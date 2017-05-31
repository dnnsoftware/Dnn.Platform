#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Services.Cryptography
{
    internal class FipsCompilanceCryptographyProvider : CryptographyProvider
    {
        /// <summary>
        ///     copy of legacy PortalSecurity.Encrypt method
        /// </summary>
        /// <param name="message">string to be encrypted</param>
        /// <param name="passphrase">key for encryption</param>
        /// <returns></returns>
        public override string EncryptParameter(string message, string passphrase)
        {
            string value;
            if (!String.IsNullOrEmpty(passphrase))
            {
                //convert key to 16 characters for simplicity
                if (passphrase.Length < 16)
                {
                    passphrase = passphrase + "XXXXXXXXXXXXXXXX".Substring(0, 16 - passphrase.Length);
                }
                else
                {
                    passphrase = passphrase.Substring(0, 16);
                }

                //create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(passphrase.Substring(0, 8));
                byte[] byteVector = Encoding.UTF8.GetBytes(passphrase.Substring(passphrase.Length - 8, 8));

                //convert data to byte array
                byte[] byteData = Encoding.UTF8.GetBytes(message);

                //encrypt 
                using (var objDes = new DESCryptoServiceProvider())
                using (var objMemoryStream = new MemoryStream())
                using (var objCryptoStream = new CryptoStream(objMemoryStream, objDes.CreateEncryptor(byteKey, byteVector),
                    CryptoStreamMode.Write))
                {
                    objCryptoStream.Write(byteData, 0, byteData.Length);
                    objCryptoStream.FlushFinalBlock();

                    //convert to string and Base64 encode
                    value = Convert.ToBase64String(objMemoryStream.ToArray());
                }
            }
            else
            {
                value = message;
            }
            return value;
        }

        /// <summary>
        ///     copy of legacy PortalSecurity.Decrypt method
        /// </summary>
        /// <param name="message">string to be decrypted</param>
        /// <param name="passphrase">key for decryption</param>
        /// <returns></returns>
        public override string DecryptParameter(string message, string passphrase)
        {
            if (String.IsNullOrEmpty(message))
            {
                return "";
            }
            string strValue = "";
            if (!String.IsNullOrEmpty(passphrase))
            {
                //convert key to 16 characters for simplicity
                if (passphrase.Length < 16)
                {
                    passphrase = passphrase + "XXXXXXXXXXXXXXXX".Substring(0, 16 - passphrase.Length);
                }
                else
                {
                    passphrase = passphrase.Substring(0, 16);
                }

                //create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(passphrase.Substring(0, 8));
                byte[] byteVector = Encoding.UTF8.GetBytes(passphrase.Substring(passphrase.Length - 8, 8));

                //convert data to byte array and Base64 decode
                var byteData = new byte[message.Length];
                try
                {
                    byteData = Convert.FromBase64String(message);
                }
                catch //invalid length
                {
                    strValue = message;
                }
                if (String.IsNullOrEmpty(strValue))
                {
                    try
                    {
                        //decrypt
                        using (var objDes = new DESCryptoServiceProvider())
                        using (var objMemoryStream = new MemoryStream())
                        using (var objCryptoStream = new CryptoStream(objMemoryStream,
                            objDes.CreateDecryptor(byteKey, byteVector), CryptoStreamMode.Write))
                        {
                            objCryptoStream.Write(byteData, 0, byteData.Length);
                            objCryptoStream.FlushFinalBlock();

                            //convert to string
                            Encoding objEncoding = Encoding.UTF8;
                            strValue = objEncoding.GetString(objMemoryStream.ToArray());
                        }
                    }
                    catch //decryption error
                    {
                        strValue = "";
                    }
                }
            }
            else
            {
                strValue = message;
            }
            return strValue;
        }

        /// <summary>
        ///     copy of legacy PortalSecurity.EncryptString method
        /// </summary>
        /// <param name="message">string to be encrypted</param>
        /// <param name="passphrase">key for encryption</param>
        /// <returns></returns>
        public override string EncryptString(string message, string passphrase)
        {
            byte[] results;
            var utf8 = new UTF8Encoding();

            using (var hashProvider = CryptographyUtils.CreateSHA512())
            {
                byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(passphrase));
                byte[] trimmedBytes = new byte[24];
                Buffer.BlockCopy(tdesKey, 0, trimmedBytes, 0, 24);
                var tdesAlgorithm = new TripleDESCryptoServiceProvider
                {
                    Key = trimmedBytes,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                byte[] dataToEncrypt = utf8.GetBytes(message);

                try
                {
                    ICryptoTransform encryptor = tdesAlgorithm.CreateEncryptor();
                    results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
                }
                finally
                {
                    // Clear the TripleDes and Hashprovider services of any sensitive information 
                    tdesAlgorithm.Clear();
                    hashProvider.Clear();
                }
            }

            //Return the encrypted string as a base64 encoded string 
            return Convert.ToBase64String(results);
        }

        /// <summary>
        ///     copy of legacy PortalSecurity.DecryptString method
        /// </summary>
        /// <param name="message">string to be decrypted</param>
        /// <param name="passphrase">key for decryption</param>
        /// <returns></returns>
        public override string DecryptString(string message, string passphrase)
        {
            byte[] results;
            var utf8 = new UTF8Encoding();

            //hash the passphrase using MD5 to create 128bit byte array
            using (var hashProvider = CryptographyUtils.CreateSHA512())
            {
                byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(passphrase));
                byte[] trimmedBytes = new byte[24];
                Buffer.BlockCopy(tdesKey, 0, trimmedBytes, 0, 24);
                var tdesAlgorithm = new TripleDESCryptoServiceProvider
                {
                    Key = trimmedBytes,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };


                byte[] dataToDecrypt = Convert.FromBase64String(message);
                try
                {
                    ICryptoTransform decryptor = tdesAlgorithm.CreateDecryptor();
                    results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
                }
                finally
                {
                    // Clear the TripleDes and Hashprovider services of any sensitive information 
                    tdesAlgorithm.Clear();
                    hashProvider.Clear();
                }
            }

            return utf8.GetString(results);
        }
    }
}