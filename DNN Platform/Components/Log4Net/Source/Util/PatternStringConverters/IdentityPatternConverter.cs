using System;
using System.IO;
using System.Security;
using System.Threading;

namespace log4net.Util.PatternStringConverters
{
	internal sealed class IdentityPatternConverter : PatternConverter
	{
		private readonly static Type declaringType;

		static IdentityPatternConverter()
		{
			IdentityPatternConverter.declaringType = typeof(IdentityPatternConverter);
		}

		public IdentityPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, object state)
		{
			try
			{
				if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null && Thread.CurrentPrincipal.Identity.Name != null)
				{
					writer.Write(Thread.CurrentPrincipal.Identity.Name);
				}
			}
			catch (SecurityException securityException)
			{
				LogLog.Debug(IdentityPatternConverter.declaringType, "Security exception while trying to get current thread principal. Error Ignored.");
				writer.Write(SystemInfo.NotAvailableText);
			}
		}
	}
}