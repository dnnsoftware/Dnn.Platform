using log4net.Core;
using log4net.Util;

using System.IO;
using System.Web;

namespace log4net.Layout.Pattern
{
	internal abstract class AspNetPatternLayoutConverter : PatternLayoutConverter
	{
		protected AspNetPatternLayoutConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			if (HttpContext.Current == null)
			{
				writer.Write(SystemInfo.NotAvailableText);
				return;
			}
			this.Convert(writer, loggingEvent, HttpContext.Current);
		}

		protected abstract void Convert(TextWriter writer, LoggingEvent loggingEvent, HttpContext httpContext);
	}
}