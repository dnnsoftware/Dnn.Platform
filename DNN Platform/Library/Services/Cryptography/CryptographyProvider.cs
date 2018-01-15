// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.ComponentModel;

namespace DotNetNuke.Services.Cryptography
{
    public abstract class CryptographyProvider : ICryptographyProvider
    {
        #region Abstract Methods

        /// <summary>
        ///     simple method that uses basic encryption to safely encode parameters
        /// </summary>
        /// <param name="message">the text to be encrypted (encoded)</param>
        /// <param name="passphrase">the key to perform the encryption</param>
        /// <returns>encrypted string</returns>
        public abstract string EncryptParameter(string message, string passphrase);

        /// <summary>
        ///     simple method that uses basic encryption to safely decode parameters
        /// </summary>
        /// <param name="message">the text to be decrypted (decoded)</param>
        /// <param name="passphrase">the key to perform the decryption</param>
        /// <returns>decrypted string</returns>
        public abstract string DecryptParameter(string message, string passphrase);

        /// <summary>
        ///     safely encrypt sensitive data
        /// </summary>
        /// <param name="message">the text to be encrypted</param>
        /// <param name="passphrase">the key to perform the encryption</param>
        /// <returns>encrypted string</returns>
        public abstract string EncryptString(string message, string passphrase);

        /// <summary>
        ///     safely decrypt sensitive data
        /// </summary>
        /// <param name="message">the text to be decrypted</param>
        /// <param name="passphrase">the key to perform the decryption</param>
        /// <returns>decrypted string</returns>
        public abstract string DecryptString(string message, string passphrase);

        #endregion

        #region static methods

        public static CryptographyProvider Instance()
        {
            return ComponentFactory.GetComponent<CryptographyProvider>();
        }

        #endregion
    }
}