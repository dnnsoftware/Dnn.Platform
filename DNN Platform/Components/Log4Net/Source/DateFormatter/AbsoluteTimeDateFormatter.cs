using System;
using System.IO;
using System.Text;

namespace log4net.DateFormatter
{
	public class AbsoluteTimeDateFormatter : IDateFormatter
	{
		public const string AbsoluteTimeDateFormat = "ABSOLUTE";

		public const string DateAndTimeDateFormat = "DATE";

		public const string Iso8601TimeDateFormat = "ISO8601";

		private static long s_lastTimeToTheSecond;

		private static StringBuilder s_lastTimeBuf;

		private static string s_lastTimeString;

		static AbsoluteTimeDateFormatter()
		{
			AbsoluteTimeDateFormatter.s_lastTimeToTheSecond = (long)0;
			AbsoluteTimeDateFormatter.s_lastTimeBuf = new StringBuilder();
		}

		public AbsoluteTimeDateFormatter()
		{
		}

		public virtual void FormatDate(DateTime dateToFormat, TextWriter writer)
		{
			long ticks = dateToFormat.Ticks - dateToFormat.Ticks % (long)10000000;
			if (AbsoluteTimeDateFormatter.s_lastTimeToTheSecond != ticks)
			{
				lock (AbsoluteTimeDateFormatter.s_lastTimeBuf)
				{
					if (AbsoluteTimeDateFormatter.s_lastTimeToTheSecond != ticks)
					{
						AbsoluteTimeDateFormatter.s_lastTimeBuf.Length = 0;
						this.FormatDateWithoutMillis(dateToFormat, AbsoluteTimeDateFormatter.s_lastTimeBuf);
						AbsoluteTimeDateFormatter.s_lastTimeString = AbsoluteTimeDateFormatter.s_lastTimeBuf.ToString();
						AbsoluteTimeDateFormatter.s_lastTimeToTheSecond = ticks;
					}
				}
			}
			writer.Write(AbsoluteTimeDateFormatter.s_lastTimeString);
			writer.Write(',');
			int millisecond = dateToFormat.Millisecond;
			if (millisecond < 100)
			{
				writer.Write('0');
			}
			if (millisecond < 10)
			{
				writer.Write('0');
			}
			writer.Write(millisecond);
		}

		protected virtual void FormatDateWithoutMillis(DateTime dateToFormat, StringBuilder buffer)
		{
			int hour = dateToFormat.Hour;
			if (hour < 10)
			{
				buffer.Append('0');
			}
			buffer.Append(hour);
			buffer.Append(':');
			int minute = dateToFormat.Minute;
			if (minute < 10)
			{
				buffer.Append('0');
			}
			buffer.Append(minute);
			buffer.Append(':');
			int second = dateToFormat.Second;
			if (second < 10)
			{
				buffer.Append('0');
			}
			buffer.Append(second);
		}
	}
}