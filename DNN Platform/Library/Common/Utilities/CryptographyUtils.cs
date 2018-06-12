#region Copyright
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
#endregion
#region Usings

using System.Security.Cryptography;
// ReSharper disable InconsistentNaming

#endregion

namespace DotNetNuke.Common.Utilities
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Common.Utilities
    /// Project:    DotNetNuke
    /// Class:      CryptographyUtils
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// CryptographyUtils is a Utility class that provides Cryptography Utility constants
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public static class CryptographyUtils
    {
        private const string _SHA1Cng = "System.Security.Cryptography.SHA1Cng";

        private const string _SHA1CryptoServiceProvider = "System.Security.Cryptography.SHA1CryptoServiceProvider";

        private const string _SHA256Cng = "System.Security.Cryptography.SHA256Cng";

        private const string _SHA256CryptoServiceProvider = "System.Security.Cryptography.SHA256CryptoServiceProvider";

        private const string _SHA384Cng = "System.Security.Cryptography.SHA384Cng";

        private const string _SHA384CryptoServiceProvider = "System.Security.Cryptography.SHA384CryptoServiceProvider";

        private const string _SHA512Cng = "System.Security.Cryptography.SHA512Cng";

        private const string _SHA512CryptoServiceProvider = "System.Security.Cryptography.SHA512CryptoServiceProvider";
        
        public static SHA1 CreateSHA1()
        {
            return SHA1.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? _SHA1CryptoServiceProvider : _SHA1Cng);
        }

        public static SHA256 CreateSHA256()
        {
            return SHA256.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? _SHA256CryptoServiceProvider : _SHA256Cng);
        }

        public static SHA384 CreateSHA384()
        {
            return SHA384.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? _SHA384CryptoServiceProvider : _SHA384Cng);
        }

        public static SHA512 CreateSHA512()
        {
            return SHA512.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? _SHA512CryptoServiceProvider : _SHA512Cng);
        }
    }
}
