using log4net.Core;
using log4net.DateFormatter;

using System;
using System.Globalization;
using System.IO;

namespace log4net.Util.PatternStringConverters
{
	internal class DatePatternConverter : PatternConverter, IOptionHandler
	{
		protected IDateFormatter m_dateFormatter;

		private readonly static Type declaringType;

		static DatePatternConverter()
		{
			DatePatternConverter.declaringType = typeof(DatePatternConverter);
		}

		public DatePatternConverter()
		{
		}

		public void ActivateOptions()
		{
			string option = this.Option ?? "ISO8601";
			if (string.Compare(option, "ISO8601", true, CultureInfo.InvariantCulture) == 0)
			{
				this.m_dateFormatter = new Iso8601DateFormatter();
				return;
			}
			if (string.Compare(option, "ABSOLUTE", true, CultureInfo.InvariantCulture) == 0)
			{
				this.m_dateFormatter = new AbsoluteTimeDateFormatter();
				return;
			}
			if (string.Compare(option, "DATE", true, CultureInfo.InvariantCulture) == 0)
			{
				this.m_dateFormatter = new DateTimeDateFormatter();
				return;
			}
			try
			{
				this.m_dateFormatter = new SimpleDateFormatter(option);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogLog.Error(DatePatternConverter.declaringType, string.Concat("Could not instantiate SimpleDateFormatter with [", option, "]"), exception);
				this.m_dateFormatter = new Iso8601DateFormatter();
			}
		}

		protected override void Convert(TextWriter writer, object state)
		{
			try
			{
				this.m_dateFormatter.FormatDate(DateTime.Now, writer);
			}
			catch (Exception exception)
			{
				LogLog.Error(DatePatternConverter.declaringType, "Error occurred while converting date.", exception);
			}
		}
	}
}