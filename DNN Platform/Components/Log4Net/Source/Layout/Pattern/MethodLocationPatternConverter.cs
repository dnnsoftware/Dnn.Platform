using log4net.Core;

using System.IO;

namespace log4net.Layout.Pattern
{
	internal sealed class MethodLocationPatternConverter : PatternLayoutConverter
	{
		public MethodLocationPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			writer.Write(loggingEvent.LocationInformation.MethodName);
		}
	}
}