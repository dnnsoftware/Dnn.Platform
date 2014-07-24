using log4net.Core;

using System.IO;

namespace log4net.Layout.Pattern
{
	internal sealed class LineLocationPatternConverter : PatternLayoutConverter
	{
		public LineLocationPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			writer.Write(loggingEvent.LocationInformation.LineNumber);
		}
	}
}