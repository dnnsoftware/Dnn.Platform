using log4net.Core;
using log4net.Util;

using System.IO;
using System.Web;

namespace log4net.Layout.Pattern
{
	internal sealed class AspNetRequestPatternConverter : AspNetPatternLayoutConverter
	{
		public AspNetRequestPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent, HttpContext httpContext)
		{
			if (httpContext.Request == null)
			{
				writer.Write(SystemInfo.NotAvailableText);
				return;
			}
			if (this.Option == null)
			{
				PatternConverter.WriteObject(writer, loggingEvent.Repository, httpContext.Request.Params);
				return;
			}
			PatternConverter.WriteObject(writer, loggingEvent.Repository, httpContext.Request.Params[this.Option]);
		}
	}
}