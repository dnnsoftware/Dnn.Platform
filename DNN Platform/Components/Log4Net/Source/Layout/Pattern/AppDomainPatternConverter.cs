using log4net.Core;

using System.IO;

namespace log4net.Layout.Pattern
{
	internal sealed class AppDomainPatternConverter : PatternLayoutConverter
	{
		public AppDomainPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			writer.Write(loggingEvent.Domain);
		}
	}
}