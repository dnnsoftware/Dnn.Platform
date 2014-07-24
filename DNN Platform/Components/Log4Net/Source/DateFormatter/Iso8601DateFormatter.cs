using System;
using System.Text;

namespace log4net.DateFormatter
{
	public class Iso8601DateFormatter : AbsoluteTimeDateFormatter
	{
		public Iso8601DateFormatter()
		{
		}

		protected override void FormatDateWithoutMillis(DateTime dateToFormat, StringBuilder buffer)
		{
			buffer.Append(dateToFormat.Year);
			buffer.Append('-');
			int month = dateToFormat.Month;
			if (month < 10)
			{
				buffer.Append('0');
			}
			buffer.Append(month);
			buffer.Append('-');
			int day = dateToFormat.Day;
			if (day < 10)
			{
				buffer.Append('0');
			}
			buffer.Append(day);
			buffer.Append(' ');
			base.FormatDateWithoutMillis(dateToFormat, buffer);
		}
	}
}