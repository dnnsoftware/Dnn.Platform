using System;
using System.Security.Cryptography;
using System.Text;

namespace DotNetNuke.Services.Exceptions
{
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
            if (!string.IsNullOrEmpty(ex.Message)) sb.AppendLine(ex.Message);
            if (!string.IsNullOrEmpty(ex.StackTrace)) sb.AppendLine(ex.StackTrace);
        }
    }
}
