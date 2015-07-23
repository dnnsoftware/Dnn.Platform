using System;
using System.Security.Cryptography;
using System.Text;

namespace DotNetNuke.Services.Exceptions
{
    public static class ExceptionExtensions
    {
        public static string Hash(this Exception exc)
        {
            var ex = exc ?? new Exception();
            var sb = new StringBuilder()
                .AppendLine(DateTime.UtcNow.ToString("O")) // make it unique in case two exact exceptions were thrown
                .AppendLine(ex.Source)
                .AppendLine(ex.ToString());

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.AppendLine(ex.ToString());
            }

            using (var hasher = new MD5CryptoServiceProvider())
            {
                var byteArray = hasher.ComputeHash(Encoding.Unicode.GetBytes(sb.ToString()));
                return Convert.ToBase64String(byteArray);
            }
        }
    }
}
