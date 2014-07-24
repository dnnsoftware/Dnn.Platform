using System;
using System.Globalization;
using System.Text;

namespace log4net.DateFormatter
{
	public class DateTimeDateFormatter : AbsoluteTimeDateFormatter
	{
		private readonly DateTimeFormatInfo m_dateTimeFormatInfo;

		public DateTimeDateFormatter()
		{
			this.m_dateTimeFormatInfo = DateTimeFormatInfo.InvariantInfo;
		}

		protected override void FormatDateWithoutMillis(DateTime dateToFormat, StringBuilder buffer)
		{
			int day = dateToFormat.Day;
			if (day < 10)
			{
				buffer.Append('0');
			}
			buffer.Append(day);
			buffer.Append(' ');
			buffer.Append(this.m_dateTimeFormatInfo.GetAbbreviatedMonthName(dateToFormat.Month));
			buffer.Append(' ');
			buffer.Append(dateToFormat.Year);
			buffer.Append(' ');
			base.FormatDateWithoutMillis(dateToFormat, buffer);
		}
	}
}