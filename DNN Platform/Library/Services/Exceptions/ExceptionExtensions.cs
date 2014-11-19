using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DotNetNuke.Services.Exceptions
{
	public static class ExceptionExtensions
	{
		public static string Hash(this Exception exc)
		{
			var sb = new StringBuilder();
			sb.AppendLine(exc.Message);
			sb.AppendLine(exc.StackTrace);
			if (exc.InnerException != null)
			{
				sb.AppendLine(exc.InnerException.Message);
				sb.AppendLine(exc.InnerException.StackTrace);
			}
			sb.AppendLine(exc.Source);
			using (var hasher = new MD5CryptoServiceProvider())
			{
				var byteArray = hasher.ComputeHash(Encoding.Unicode.GetBytes(sb.ToString()));
				return byteArray.Aggregate("", (current, b) => current + b.ToString("x2"));
			}
		}
	}
}
