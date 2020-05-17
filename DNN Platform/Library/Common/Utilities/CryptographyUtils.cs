﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
