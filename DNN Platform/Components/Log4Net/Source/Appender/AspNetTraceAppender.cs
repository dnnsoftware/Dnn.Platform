using log4net.Core;
using log4net.Layout;

using System.Web;

namespace log4net.Appender
{
	public class AspNetTraceAppender : AppenderSkeleton
	{
		private PatternLayout m_category = new PatternLayout("%logger");

		public PatternLayout Category
		{
			get
			{
				return this.m_category;
			}
			set
			{
				this.m_category = value;
			}
		}

		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		public AspNetTraceAppender()
		{
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			if (HttpContext.Current != null && HttpContext.Current.Trace.IsEnabled)
			{
				if (loggingEvent.Level >= Level.Warn)
				{
					HttpContext.Current.Trace.Warn(this.m_category.Format(loggingEvent), base.RenderLoggingEvent(loggingEvent));
					return;
				}
				HttpContext.Current.Trace.Write(this.m_category.Format(loggingEvent), base.RenderLoggingEvent(loggingEvent));
			}
		}
	}
}