using log4net.Core;

using System.Globalization;
using System.IO;

namespace log4net.Layout
{
	public abstract class LayoutSkeleton : ILayout, IOptionHandler
	{
		private string m_header;

		private string m_footer;

		private bool m_ignoresException = true;

		public virtual string ContentType
		{
			get
			{
				return "text/plain";
			}
		}

		public virtual string Footer
		{
			get
			{
				return this.m_footer;
			}
			set
			{
				this.m_footer = value;
			}
		}

		public virtual string Header
		{
			get
			{
				return this.m_header;
			}
			set
			{
				this.m_header = value;
			}
		}

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

		protected LayoutSkeleton()
		{
		}

		public abstract void ActivateOptions();

		public abstract void Format(TextWriter writer, LoggingEvent loggingEvent);

		public string Format(LoggingEvent loggingEvent)
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			this.Format(stringWriter, loggingEvent);
			return stringWriter.ToString();
		}
	}
}