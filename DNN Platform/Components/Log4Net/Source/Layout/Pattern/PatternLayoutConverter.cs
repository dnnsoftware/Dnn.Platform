using log4net.Core;
using log4net.Util;
using System;
using System.IO;

namespace log4net.Layout.Pattern
{
	public abstract class PatternLayoutConverter : PatternConverter
	{
		private bool m_ignoresException = true;

		public virtual bool IgnoresException
		{
			get
			{
				return this.m_ignoresException;
			}
			set
			{
				this.m_ignoresException = value;
			}
		}

		protected PatternLayoutConverter()
		{
		}

		protected abstract void Convert(TextWriter writer, LoggingEvent loggingEvent);

		protected override void Convert(TextWriter writer, object state)
		{
			LoggingEvent loggingEvent = state as LoggingEvent;
			if (loggingEvent == null)
			{
				throw new ArgumentException(string.Concat("state must be of type [", typeof(LoggingEvent).FullName, "]"), "state");
			}
			this.Convert(writer, loggingEvent);
		}
	}
}