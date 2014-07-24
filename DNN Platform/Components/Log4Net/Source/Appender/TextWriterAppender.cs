using log4net.Core;
using log4net.Layout;
using log4net.Util;
using System;
using System.IO;

namespace log4net.Appender
{
	public class TextWriterAppender : AppenderSkeleton
	{
		private QuietTextWriter m_qtw;

		private bool m_immediateFlush = true;

		private readonly static Type declaringType;

		public override IErrorHandler ErrorHandler
		{
			get
			{
				return base.ErrorHandler;
			}
			set
			{
				lock (this)
				{
					if (value != null)
					{
						base.ErrorHandler = value;
						if (this.m_qtw != null)
						{
							this.m_qtw.ErrorHandler = value;
						}
					}
					else
					{
						LogLog.Warn(TextWriterAppender.declaringType, "TextWriterAppender: You have tried to set a null error-handler.");
					}
				}
			}
		}

		public bool ImmediateFlush
		{
			get
			{
				return this.m_immediateFlush;
			}
			set
			{
				this.m_immediateFlush = value;
			}
		}

		protected QuietTextWriter QuietWriter
		{
			get
			{
				return this.m_qtw;
			}
			set
			{
				this.m_qtw = value;
			}
		}

		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		public virtual TextWriter Writer
		{
			get
			{
				return this.m_qtw;
			}
			set
			{
				lock (this)
				{
					this.Reset();
					if (value != null)
					{
						this.m_qtw = new QuietTextWriter(value, this.ErrorHandler);
						this.WriteHeader();
					}
				}
			}
		}

		static TextWriterAppender()
		{
			TextWriterAppender.declaringType = typeof(TextWriterAppender);
		}

		public TextWriterAppender()
		{
		}

		[Obsolete("Instead use the default constructor and set the Layout & Writer properties")]
		public TextWriterAppender(ILayout layout, Stream os) : this(layout, new StreamWriter(os))
		{
		}

		[Obsolete("Instead use the default constructor and set the Layout & Writer properties")]
		public TextWriterAppender(ILayout layout, TextWriter writer)
		{
			this.Layout = layout;
			this.Writer = writer;
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			base.RenderLoggingEvent(this.m_qtw, loggingEvent);
			if (this.m_immediateFlush)
			{
				this.m_qtw.Flush();
			}
		}

		protected override void Append(LoggingEvent[] loggingEvents)
		{
			LoggingEvent[] loggingEventArray = loggingEvents;
			for (int i = 0; i < (int)loggingEventArray.Length; i++)
			{
				LoggingEvent loggingEvent = loggingEventArray[i];
				base.RenderLoggingEvent(this.m_qtw, loggingEvent);
			}
			if (this.m_immediateFlush)
			{
				this.m_qtw.Flush();
			}
		}

		protected virtual void CloseWriter()
		{
			if (this.m_qtw != null)
			{
				try
				{
					this.m_qtw.Close();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.ErrorHandler.Error(string.Concat("Could not close writer [", this.m_qtw, "]"), exception);
				}
			}
		}

		protected override void OnClose()
		{
			lock (this)
			{
				this.Reset();
			}
		}

		protected override bool PreAppendCheck()
		{
			if (!base.PreAppendCheck())
			{
				return false;
			}
			if (this.m_qtw == null)
			{
				this.PrepareWriter();
				if (this.m_qtw == null)
				{
					this.ErrorHandler.Error(string.Concat("No output stream or file set for the appender named [", base.Name, "]."));
					return false;
				}
			}
			if (!this.m_qtw.Closed)
			{
				return true;
			}
			this.ErrorHandler.Error(string.Concat("Output stream for appender named [", base.Name, "] has been closed."));
			return false;
		}

		protected virtual void PrepareWriter()
		{
		}

		protected virtual void Reset()
		{
			this.WriteFooterAndCloseWriter();
			this.m_qtw = null;
		}

		protected virtual void WriteFooter()
		{
			if (this.Layout != null && this.m_qtw != null && !this.m_qtw.Closed)
			{
				string footer = this.Layout.Footer;
				if (footer != null)
				{
					this.m_qtw.Write(footer);
				}
			}
		}

		protected virtual void WriteFooterAndCloseWriter()
		{
			this.WriteFooter();
			this.CloseWriter();
		}

		protected virtual void WriteHeader()
		{
			if (this.Layout != null && this.m_qtw != null && !this.m_qtw.Closed)
			{
				string header = this.Layout.Header;
				if (header != null)
				{
					this.m_qtw.Write(header);
				}
			}
		}
	}
}