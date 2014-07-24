using log4net.Core;
using log4net.Layout;
using System;
using System.Globalization;

namespace log4net.Appender
{
	public class ConsoleAppender : AppenderSkeleton
	{
		public const string ConsoleOut = "Console.Out";

		public const string ConsoleError = "Console.Error";

		private bool m_writeToErrorStream;

		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		public virtual string Target
		{
			get
			{
				if (!this.m_writeToErrorStream)
				{
					return "Console.Out";
				}
				return "Console.Error";
			}
			set
			{
				if (string.Compare("Console.Error", value.Trim(), true, CultureInfo.InvariantCulture) == 0)
				{
					this.m_writeToErrorStream = true;
					return;
				}
				this.m_writeToErrorStream = false;
			}
		}

		public ConsoleAppender()
		{
		}

		[Obsolete("Instead use the default constructor and set the Layout property")]
		public ConsoleAppender(ILayout layout) : this(layout, false)
		{
		}

		[Obsolete("Instead use the default constructor and set the Layout & Target properties")]
		public ConsoleAppender(ILayout layout, bool writeToErrorStream)
		{
			this.Layout = layout;
			this.m_writeToErrorStream = writeToErrorStream;
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			if (!this.m_writeToErrorStream)
			{
				Console.Write(base.RenderLoggingEvent(loggingEvent));
				return;
			}
			Console.Error.Write(base.RenderLoggingEvent(loggingEvent));
		}
	}
}