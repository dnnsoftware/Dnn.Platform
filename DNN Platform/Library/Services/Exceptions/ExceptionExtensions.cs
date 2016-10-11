using System;
using System.Security.Cryptography;
using System.Text;
using DotNetNuke.Common;

namespace DotNetNuke.Services.Exceptions
{
    public static class ExceptionExtensions
    {
        // DNN-8845: using a FIPS compliant HashAlgorithm
        private static readonly HashAlgorithm Hasher = new HMACSHA1();

        public static string Hash(this Exception exc)
        {
            // the timestamp enforces unique hash when two exact exceptions are thrown
            var timestamp = DateTime.UtcNow + Globals.ElapsedSinceAppStart;
            var sb = new StringBuilder().AppendLine(timestamp.ToString("O"));
            while (exc != null)
            {
                sb.AppendLine(exc.ToString());
                exc = exc.InnerException;
            }
            return Convert.ToBase64String(Hasher.ComputeHash(Encoding.Unicode.GetBytes(sb.ToString())));
        }
    }
}
