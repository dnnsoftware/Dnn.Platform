using log4net.Core;
using log4net.Util;

using System.IO;
using System.Web;

namespace log4net.Layout.Pattern
{
	internal sealed class AspNetCachePatternConverter : AspNetPatternLayoutConverter
	{
		public AspNetCachePatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent, HttpContext httpContext)
		{
			if (HttpRuntime.Cache == null)
			{
				writer.Write(SystemInfo.NotAvailableText);
				return;
			}
			if (this.Option == null)
			{
				PatternConverter.WriteObject(writer, loggingEvent.Repository, HttpRuntime.Cache.GetEnumerator());
				return;
			}
			PatternConverter.WriteObject(writer, loggingEvent.Repository, HttpRuntime.Cache[this.Option]);
		}
	}
}