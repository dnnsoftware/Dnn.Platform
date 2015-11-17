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

            if (!string.IsNullOrEmpty(exc.Message))
            {
                sb.AppendLine(exc.Message);
            }

            if (!string.IsNullOrEmpty(exc.StackTrace))
            {
                sb.AppendLine(exc.StackTrace);
            }

            if (exc.InnerException != null)
            {
                if (!string.IsNullOrEmpty(exc.InnerException.Message))
                {
                    sb.AppendLine(exc.InnerException.Message);
                }

                if (!string.IsNullOrEmpty(exc.InnerException.StackTrace))
                {
                    sb.AppendLine(exc.InnerException.StackTrace);
                }
            }

            using (var hasher = new MD5CryptoServiceProvider())
            {
                var byteArray = hasher.ComputeHash(Encoding.Unicode.GetBytes(sb.ToString().ToLower()));
                return Convert.ToBase64String(byteArray);
            }
        }
    }
}
