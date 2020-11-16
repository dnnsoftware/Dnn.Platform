// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class ExceptionExtensions
    {
        public static string Hash(this Exception exc)
        {
            if (exc == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            AddException(sb, exc);

            if (exc.InnerException != null)
            {
                AddException(sb, exc.InnerException);
            }

            // DNN-8845: using a FIPS compliant HashAlgorithm
            using (var hasher = new SHA1CryptoServiceProvider())
            {
                var byteArray = hasher.ComputeHash(Encoding.Unicode.GetBytes(sb.ToString().ToLowerInvariant()));
                return Convert.ToBase64String(byteArray);
            }
        }

        private static void AddException(StringBuilder sb, Exception ex)
        {
            if (!string.IsNullOrEmpty(ex.Message))
            {
                sb.AppendLine(ex.Message);
            }

            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                sb.AppendLine(ex.StackTrace);
            }
        }
    }
}
