using log4net.Core;

using System.IO;

namespace log4net.Layout.Pattern
{
	internal sealed class LevelPatternConverter : PatternLayoutConverter
	{
		public LevelPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			writer.Write(loggingEvent.Level.DisplayName);
		}
	}
}