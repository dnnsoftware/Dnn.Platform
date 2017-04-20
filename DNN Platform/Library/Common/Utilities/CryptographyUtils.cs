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
#region Usings

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Services.Upgrade;
using System.Collections.Generic;

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
    public class CryptographyUtils
    {
        public static string SHA1 => "System.Security.Cryptography.SHA1";

        public static string SHA1Cng => "System.Security.Cryptography.SHA1Cng";

        public static string SHA1CryptoServiceProvider => "System.Security.Cryptography.SHA1CryptoServiceProvider";

        public static string MD5 => "System.Security.Cryptography.MD5";

        public static string MD5Cng => "System.Security.Cryptography.MD5Cng";

        public static string MD5CryptoServiceProvider => "System.Security.Cryptography.MD5CryptoServiceProvider";

        public static string SHA256 => "System.Security.Cryptography.SHA256";

        public static string SHA256Cng => "System.Security.Cryptography.SHA256Cng";

        public static string SHA256CryptoServiceProvider => "System.Security.Cryptography.SHA256CryptoServiceProvider";
    }
}
