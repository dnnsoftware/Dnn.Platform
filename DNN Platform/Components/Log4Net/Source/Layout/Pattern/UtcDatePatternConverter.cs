using log4net.Core;
using log4net.Util;
using System;
using System.IO;

namespace log4net.Layout.Pattern
{
	internal class UtcDatePatternConverter : DatePatternConverter
	{
		private readonly static Type declaringType;

		static UtcDatePatternConverter()
		{
			UtcDatePatternConverter.declaringType = typeof(UtcDatePatternConverter);
		}

		public UtcDatePatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			try
			{
				this.m_dateFormatter.FormatDate(loggingEvent.TimeStamp.ToUniversalTime(), writer);
			}
			catch (Exception exception)
			{
				LogLog.Error(UtcDatePatternConverter.declaringType, "Error occurred while converting date.", exception);
			}
		}
	}
}