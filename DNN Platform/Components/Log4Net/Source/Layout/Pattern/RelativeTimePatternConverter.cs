using log4net.Core;
using System;
using System.Globalization;
using System.IO;

namespace log4net.Layout.Pattern
{
	internal sealed class RelativeTimePatternConverter : PatternLayoutConverter
	{
		public RelativeTimePatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			long num = RelativeTimePatternConverter.TimeDifferenceInMillis(LoggingEvent.StartTime, loggingEvent.TimeStamp);
			writer.Write(num.ToString(NumberFormatInfo.InvariantInfo));
		}

		private static long TimeDifferenceInMillis(DateTime start, DateTime end)
		{
			TimeSpan universalTime = end.ToUniversalTime() - start.ToUniversalTime();
			return (long)universalTime.TotalMilliseconds;
		}
	}
}