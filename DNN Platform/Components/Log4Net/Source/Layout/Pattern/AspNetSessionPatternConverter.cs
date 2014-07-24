using log4net.Core;
using log4net.Util;

using System.IO;
using System.Web;

namespace log4net.Layout.Pattern
{
	internal sealed class AspNetSessionPatternConverter : AspNetPatternLayoutConverter
	{
		public AspNetSessionPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent, HttpContext httpContext)
		{
			if (httpContext.Session == null)
			{
				writer.Write(SystemInfo.NotAvailableText);
				return;
			}
			if (this.Option == null)
			{
				PatternConverter.WriteObject(writer, loggingEvent.Repository, httpContext.Session);
				return;
			}
			PatternConverter.WriteObject(writer, loggingEvent.Repository, httpContext.Session.Contents[this.Option]);
		}
	}
}