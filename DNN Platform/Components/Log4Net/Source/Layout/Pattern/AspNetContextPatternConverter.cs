using log4net.Core;
using log4net.Util;

using System.IO;
using System.Web;

namespace log4net.Layout.Pattern
{
	internal sealed class AspNetContextPatternConverter : AspNetPatternLayoutConverter
	{
		public AspNetContextPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent, HttpContext httpContext)
		{
			if (this.Option == null)
			{
				PatternConverter.WriteObject(writer, loggingEvent.Repository, httpContext.Items);
				return;
			}
			PatternConverter.WriteObject(writer, loggingEvent.Repository, httpContext.Items[this.Option]);
		}
	}
}