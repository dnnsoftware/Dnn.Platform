using log4net.Core;

namespace log4net.Layout.Pattern
{
	internal sealed class LoggerPatternConverter : NamedPatternConverter
	{
		public LoggerPatternConverter()
		{
		}

		protected override string GetFullyQualifiedName(LoggingEvent loggingEvent)
		{
			return loggingEvent.LoggerName;
		}
	}
}