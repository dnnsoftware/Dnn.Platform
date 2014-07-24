using System;
using System.IO;

namespace log4net.Util.PatternStringConverters
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

		protected override void Convert(TextWriter writer, object state)
		{
			try
			{
				this.m_dateFormatter.FormatDate(DateTime.UtcNow, writer);
			}
			catch (Exception exception)
			{
				LogLog.Error(UtcDatePatternConverter.declaringType, "Error occurred while converting date.", exception);
			}
		}
	}
}